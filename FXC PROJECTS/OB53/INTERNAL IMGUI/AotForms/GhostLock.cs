using System;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace AotForms
{
    /// <summary>
    /// ╔══════════════════════════════════════════════════════════════╗
    /// ║              G H O S T L O C K  v1.1                         ║
    /// ║         Predictive Adaptive Aim Engine — by FearMods         ║
    /// ╠══════════════════════════════════════════════════════════════╣
    /// ║  UNIQUE FEATURES:                                            ║
    /// ║  1. Velocity Prediction — aims WHERE target WILL BE          ║
    /// ║  2. Adaptive Angular Slerp — far=fast snap, near=butter      ║
    /// ║  3. Hysteresis Confidence Lock — zero target flicker         ║
    /// ║  4. Multi-Bone Smart Selection — auto sniper/AR/CQC mode     ║
    /// ║  5. Composite Priority Score — FOV + distance + health       ║
    /// ║  6. No Dictionary generics — AOT safe                        ║
    /// ╚══════════════════════════════════════════════════════════════╝
    /// </summary>
    internal static class GhostLock
    {
        // ─── Velocity Tracker (Array-based, no Dictionary) ────────────────────
        // Fixed slot count — AOT compiler loves simple arrays, no generic cycles
        private const int SLOT_COUNT = 64;

        private static readonly uint[]   _slotAddr = new uint[SLOT_COUNT];
        private static readonly Vector3[] _slotPrev = new Vector3[SLOT_COUNT];
        private static readonly Vector3[] _slotVel  = new Vector3[SLOT_COUNT];
        private static readonly bool[]    _slotUsed = new bool[SLOT_COUNT];

        // ─── Hysteresis Lock State ─────────────────────────────────────────────
        private static Entity  _lockedTarget   = null;
        private static float   _lockConfidence = 0f;

        private const float CONFIDENCE_GAIN  = 0.55f;
        private const float CONFIDENCE_DECAY = 0.20f;
        private const float CONFIDENCE_MIN   = 0.0f;
        private const float CONFIDENCE_START = 0.55f;

        // ─── Smooth Rotation State ─────────────────────────────────────────────
        private static Quaternion _smoothRot = Quaternion.Identity;

        // ─── Prediction Tuning ─────────────────────────────────────────────────
        private const float PREDICT_NORMAL = 0.10f;
        private const float PREDICT_SNIPER = 0.22f;

        // ─── Main Loop ─────────────────────────────────────────────────────────
        internal static void Work()
        {
            while (true)
            {
                // AGENTS.md Rule 1: while(true) — thread kabhi nahi marega
                if (!Config.GhostLock || Core.LocalPlayer == 0)
                {
                    _Reset();
                    Thread.Sleep(5);
                    continue;
                }

                if ((WinAPI.GetAsyncKeyState(Config.AimbotKey) & 0x8000) == 0)
                {
                    _SoftReset();
                    Thread.Sleep(2);
                    continue;
                }

                if (!Core.HaveMatrix || Core.Width <= 0 || Core.Height <= 0)
                {
                    Thread.Sleep(2);
                    continue;
                }

                // Step 1: Update velocity slots
                _UpdateVelocities();

                // Step 2: Auto-detect sniper mode
                bool sniper = Config.GhostLockSniperMode || Config.AimFov < 80f;

                // Step 3: Score best target
                var screenCenter = new Vector2(Core.Width / 2f, Core.Height / 2f);
                Entity scored = _FindBestTarget(screenCenter, sniper);

                // Step 4: Hysteresis — flicker-proof lock
                Entity finalTarget = _ApplyHysteresis(scored);

                if (finalTarget == null) { Thread.Sleep(2); continue; }

                // Step 5: Predicted aim position
                Vector3 aimPos = _GetPredictedAimPos(finalTarget, sniper);

                // Step 6: Target rotation
                var targetRot = MathUtils.GetRotationToLocation(aimPos, 0.1f, Core.LocalMainCamera);

                // Step 7: Adaptive angular Slerp
                float lerpT = _CalcAdaptiveLerp(targetRot, sniper);

                if (InternalMemory.Read<Quaternion>(Core.LocalPlayer + Offsets.AimRotation, out var curRot))
                    _smoothRot = Quaternion.Slerp(curRot, targetRot, lerpT);
                else
                    _smoothRot = targetRot;

                InternalMemory.Write(Core.LocalPlayer + Offsets.AimRotation, _smoothRot);

                Thread.Sleep(sniper ? 4 : 1);
            }
        }

        // ─── VELOCITY TRACKER (Array-based, no Dictionary) ────────────────────
        private static void _UpdateVelocities()
        {
            var snapshot = Core.Entities.Values.ToArray();

            foreach (var e in snapshot)
            {
                if (e == null || !e.IsKnown || e.IsDead) continue;

                uint id  = e.Address;
                int  slot = (int)(id % SLOT_COUNT);

                Vector3 cur = e.Head;

                if (_slotUsed[slot] && _slotAddr[slot] == id)
                {
                    Vector3 rawVel = cur - _slotPrev[slot];
                    if (rawVel.Length() < 5.0f)  // teleport filter
                    {
                        _slotVel[slot] = Vector3.Lerp(_slotVel[slot], rawVel, 0.4f);
                    }
                }

                _slotAddr[slot] = id;
                _slotPrev[slot] = cur;
                _slotUsed[slot] = true;
            }
        }

        // ─── GET VELOCITY FOR ENTITY ──────────────────────────────────────────
        private static Vector3 _GetVelocity(uint id)
        {
            int slot = (int)(id % SLOT_COUNT);
            if (_slotUsed[slot] && _slotAddr[slot] == id)
                return _slotVel[slot];
            return Vector3.Zero;
        }

        // ─── PREDICTED AIM POSITION ──────────────────────────────────────────
        private static Vector3 _GetPredictedAimPos(Entity e, bool sniper)
        {
            Vector3 basePos = _SelectBone(e, sniper);

            Vector3 vel = _GetVelocity(e.Address);
            if (vel != Vector3.Zero)
            {
                float factor    = sniper ? PREDICT_SNIPER : PREDICT_NORMAL;
                float worldDist = Vector3.Distance(Core.LocalMainCamera, basePos);
                float distScale = Math.Clamp(worldDist / 100f, 0.5f, 2.5f);
                basePos += vel * factor * distScale;
            }

            return basePos;
        }

        // ─── BONE SELECTOR ───────────────────────────────────────────────────
        private static Vector3 _SelectBone(Entity e, bool sniper)
        {
            if (sniper) return e.Spine; // chest — bigger hitbox for snipers

            return Config.AimTargetPart switch
            {
                "HEAD"      => e.Head,
                "NECK"      => e.Neck,
                "CHEST"     => e.Spine,
                "HIP"       => e.Hip,
                "LSHOULDER" => e.LeftShoulder,
                "RSHOULDER" => e.RightShoulder,
                _           => e.Head
            };
        }

        // ─── MULTI-FACTOR TARGET SCORING ─────────────────────────────────────
        private static Entity _FindBestTarget(Vector2 screenCenter, bool sniper)
        {
            Entity best      = null;
            float  bestScore = float.MaxValue;
            var    snapshot  = Core.Entities.Values.ToArray();

            foreach (var e in snapshot)
            {
                if (!_IsValid(e)) continue;

                Vector3 predictedPos = _GetPredictedAimPos(e, sniper);

                float worldDist = Vector3.Distance(Core.LocalMainCamera, predictedPos);
                if (worldDist > Config.AimBotMaxDistance) continue;

                var screen = W2S.WorldToScreen(Core.CameraMatrix, predictedPos, Core.Width, Core.Height);
                if (screen.X < 1 || screen.Y < 1) continue;

                float dx      = screen.X - screenCenter.X;
                float dy      = screen.Y - screenCenter.Y;
                float fovDist = MathF.Sqrt(dx * dx + dy * dy);

                if (fovDist > Config.AimFov) continue;

                // Composite score: FOV dist + world dist factor + knocked penalty
                float score = fovDist
                            + (worldDist / Config.AimBotMaxDistance) * 30f
                            + (e.IsKnocked ? 20f : 0f);

                if (score < bestScore) { bestScore = score; best = e; }
            }

            return best;
        }

        // ─── HYSTERESIS CONFIDENCE LOCK ──────────────────────────────────────
        private static Entity _ApplyHysteresis(Entity scored)
        {
            if (scored == null)
            {
                _lockConfidence = Math.Max(CONFIDENCE_MIN, _lockConfidence - CONFIDENCE_DECAY);
                if (_lockConfidence <= CONFIDENCE_MIN) _lockedTarget = null;
                return _lockedTarget;
            }

            if (_lockedTarget == null || !_IsValid(_lockedTarget))
            {
                _lockedTarget   = scored;
                _lockConfidence = CONFIDENCE_START;
                return _lockedTarget;
            }

            if (scored == _lockedTarget)
            {
                _lockConfidence = Math.Min(1f, _lockConfidence + CONFIDENCE_GAIN);
                return _lockedTarget;
            }

            _lockConfidence = Math.Max(CONFIDENCE_MIN, _lockConfidence - CONFIDENCE_DECAY);
            if (_lockConfidence <= CONFIDENCE_MIN)
            {
                _lockedTarget   = scored;
                _lockConfidence = CONFIDENCE_START;
            }

            return _lockedTarget;
        }

        // ─── ADAPTIVE ANGULAR SLERP ──────────────────────────────────────────
        private static float _CalcAdaptiveLerp(Quaternion targetRot, bool sniper)
        {
            float smooth = Math.Clamp(Config.AimBotSmooth, 1f, 50f);

            float dot    = Math.Abs(Quaternion.Dot(
                Quaternion.Normalize(_smoothRot),
                Quaternion.Normalize(targetRot)));
            float angSep = 1f - Math.Clamp(dot, 0f, 1f);

            float angFactor = Math.Clamp(MathF.Pow(angSep, 0.5f) * 3f, 0.04f, 1f);
            float lerp      = (1f / smooth) * angFactor;
            if (sniper) lerp *= 0.5f;

            return Math.Clamp(lerp, 0.01f, 0.92f);
        }

        // ─── TARGET VALIDATION ───────────────────────────────────────────────
        private static bool _IsValid(Entity e)
        {
            if (e == null)                            return false;
            if (!e.IsKnown)                           return false;
            if (e.IsDead)                             return false;
            if (e.IsTeam == Bool3.True)               return false;
            if (Config.IgnoreKnocked && e.IsKnocked)  return false;
            if (Config.AimbotVisible && !e.isVisible) return false;
            return true;
        }

        // ─── RESET ───────────────────────────────────────────────────────────
        private static void _Reset()
        {
            _lockedTarget   = null;
            _lockConfidence = 0f;
            _smoothRot      = Quaternion.Identity;
            Array.Clear(_slotUsed, 0, SLOT_COUNT);
        }

        private static void _SoftReset()
        {
            _lockedTarget   = null;
            _lockConfidence = 0f;
            _smoothRot      = Quaternion.Identity;
            // velocity slots rakho — prediction ready rehegi
        }
    }
}
