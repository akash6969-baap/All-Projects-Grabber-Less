using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TrackbarFloat
{

    [DefaultProperty("BarInnerColor")]
    [DefaultEvent("ValueChanged")]
    [ToolboxBitmap(typeof(TrackBar))]
    public sealed class TrackBarFloatClient : Control
    {

        public TrackBarFloatClient(int int_7, int int_8, int int_9)
        {
            base.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable | ControlStyles.UserMouse | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            base.Size = new Size(100, 20);
            this.Text = "";
            this.Min = int_7;
            this.Max = int_8;
            this.Value = int_9;
        }

        public TrackBarFloatClient() : this(0, 100, 10)
        {
        }


        [Category("Action")]
        [Description("Event fires when the Value property changes")]
        public event EventHandler Event_0
        {
            [CompilerGenerated]
            add
            {
                EventHandler eventHandler = this.eventHandler_0;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Combine(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_0, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
            [CompilerGenerated]
            remove
            {
                EventHandler eventHandler = this.eventHandler_0;
                EventHandler eventHandler2;
                do
                {
                    eventHandler2 = eventHandler;
                    EventHandler value2 = (EventHandler)Delegate.Remove(eventHandler2, value);
                    eventHandler = Interlocked.CompareExchange<EventHandler>(ref this.eventHandler_0, value2, eventHandler2);
                }
                while (eventHandler != eventHandler2);
            }
        }


        [Description("Event fires when the Slider position is changed")]
        [Category("Behavior")]
        public event ScrollEventHandler Event_1
        {
            [CompilerGenerated]
            add
            {
                ScrollEventHandler scrollEventHandler = this.scrollEventHandler_0;
                ScrollEventHandler scrollEventHandler2;
                do
                {
                    scrollEventHandler2 = scrollEventHandler;
                    ScrollEventHandler value2 = (ScrollEventHandler)Delegate.Combine(scrollEventHandler2, value);
                    scrollEventHandler = Interlocked.CompareExchange<ScrollEventHandler>(ref this.scrollEventHandler_0, value2, scrollEventHandler2);
                }
                while (scrollEventHandler != scrollEventHandler2);
            }
            [CompilerGenerated]
            remove
            {
                ScrollEventHandler scrollEventHandler = this.scrollEventHandler_0;
                ScrollEventHandler scrollEventHandler2;
                do
                {
                    scrollEventHandler2 = scrollEventHandler;
                    ScrollEventHandler value2 = (ScrollEventHandler)Delegate.Remove(scrollEventHandler2, value);
                    scrollEventHandler = Interlocked.CompareExchange<ScrollEventHandler>(ref this.scrollEventHandler_0, value2, scrollEventHandler2);
                }
                while (scrollEventHandler != scrollEventHandler2);
            }
        }
        [Browsable(false)]
        public Rectangle Rectangle_0
        {
            get
            {
                return this.rectangle_3;
            }
        }
        [Category("Slider - HoveredState")]
        public Color Color_0
        {
            get
            {
                return this.color_0;
            }
            set
            {
                this.color_0 = value;
                base.Invalidate();
            }
        }


        [Category("Slider - HoveredState")]
        public Color Color_1
        {
            get
            {
                return this.color_1;
            }
            set
            {
                this.color_1 = value;
                base.Invalidate();
            }
        }


        [Category("Slider - HoveredState")]
        public Color Color_2
        {
            get
            {
                return this.color_2;
            }
            set
            {
                this.color_2 = value;
                base.Invalidate();
            }
        }


        [Category("Slider")]
        public bool Boolean_0
        {
            get
            {
                return this.bool_0;
            }
            set
            {
                this.bool_0 = value;
                base.Invalidate();
            }
        }


        public float Single_0
        {
            get
            {
                return this.float_0;
            }
            set
            {
                this.float_0 = value;
                base.Invalidate();
            }
        }



        [DefaultValue(10)]
        [Category("Slider")]
        public int Value
        {
            get
            {
                return this.int_2;
            }
            set
            {
                if (value >= this.int_3 & value <= this.int_4)
                {
                    this.int_2 = value;
                    EventHandler eventHandler = this.eventHandler_0;
                    if (eventHandler != null)
                    {
                        eventHandler(this, new EventArgs());
                    }
                    base.Invalidate();
                    return;
                }
                throw new ArgumentOutOfRangeException("Value is outside appropriate range (min, max)");
            }
        }


        [DefaultValue(0)]
        [Category("Slider")]
        public int Min
        {
            get
            {
                return this.int_3;
            }
            set
            {
                if (value < this.int_4)
                {
                    this.int_3 = value;
                    if (this.int_2 < this.int_3)
                    {
                        this.int_2 = this.int_3;
                        EventHandler eventHandler = this.eventHandler_0;
                        if (eventHandler != null)
                        {
                            eventHandler(this, new EventArgs());
                        }
                    }
                    base.Invalidate();
                    return;
                }
                throw new ArgumentOutOfRangeException("Minimal value is greather than maximal one");
            }
        }


        [DefaultValue(100)]
        [Category("Slider")]
        public int Max
        {
            get
            {
                return this.int_4;
            }
            set
            {
                if (value > this.int_3)
                {
                    this.int_4 = value;
                    if (this.int_2 > this.int_4)
                    {
                        this.int_2 = this.int_4;
                        EventHandler eventHandler = this.eventHandler_0;
                        if (eventHandler != null)
                        {
                            eventHandler(this, new EventArgs());
                        }
                    }
                    base.Invalidate();
                }
            }
        }

        public float Single_1
        {
            get
            {
                return this.float_1;
            }
            set
            {
                this.float_1 = value;
                base.Invalidate();
            }
        }


        public bool Boolean_1
        {
            get
            {
                return this.bool_1;
            }
            set
            {
                this.bool_1 = value;
            }
        }


        [DefaultValue(typeof(Color), "Black")]
        [Category("Slider")]
        public Color Color_3
        {
            get
            {
                return this.color_3;
            }
            set
            {
                this.color_3 = value;
                base.Invalidate();
            }
        }


        [Category("Slider")]
        public Color Color_4
        {
            get
            {
                return this.color_4;
            }
            set
            {
                this.color_4 = value;
                base.Invalidate();
            }
        }


        public Color Color_5
        {
            get
            {
                return this.color_5;
            }
            set
            {
                this.color_5 = value;
                base.Invalidate();
            }
        }

        // Token: 0x060000F0 RID: 240 RVA: 0x00002F23 File Offset: 0x00001123
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            TrackBarFloatClient.bool_5 = true;
            TrackBarFloatClient.bool_3 = true;
            this.method_0();
        }


        private async void method_0()
        {
            bool_5 = true;
            await Task.Run(delegate
            {
                method_1();
            });
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            this.method_2(e, this.color_3);
        }


        public async void method_1()
        {
            try
            {
                bool_2 = false;
                while (bool_5)
                {
                    await Task.Delay(10);
                    int num = rectangle_3.Left + 8;
                    if (decimal_0 < (decimal)num)
                    {
                        decimal num2 = ((decimal)num - decimal_0) * 0.15m;
                        decimal_0 += num2;
                        Invalidate();
                    }
                    if (decimal_0 > (decimal)num)
                    {
                        decimal num3 = ((decimal)num - decimal_0) * 0.15m;
                        decimal_0 += num3;
                        Invalidate();
                    }
                }
            }
            finally
            {
                bool_2 = true;
            }
        }

        public int Int32_3
        {
            get
            {
                return this.int_6;
            }
            set
            {
                this.int_6 = value;
            }
        }


        private void method_2(PaintEventArgs paintEventArgs_0, Color color_6)
        {
            this.rectangle_0 = base.ClientRectangle;
            this.int_5 = (this.int_2 - this.int_3) * (base.ClientRectangle.Width - 16) / (this.int_4 - this.int_3);
            this.rectangle_3 = new Rectangle(this.int_5, this.rectangle_0.Y + base.ClientRectangle.Height / 2 - 8, 16, 16);
            this.rectangle_1 = this.rectangle_0;
            this.rectangle_1.Height = this.rectangle_1.Height / 2;
            this.rectangle_2 = this.rectangle_0;
            this.rectangle_2.Width = this.rectangle_3.Left + 8 - this.int_0;
            paintEventArgs_0.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            paintEventArgs_0.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            paintEventArgs_0.Graphics.Clear(base.Parent.BackColor);
            Pen pen = new Pen(this.color_5, this.float_1);
            Pen pen2 = new Pen(this.color_0, this.float_1);
            GraphicsPath path = Helper.smethod_0((float)this.rectangle_0.X, (float)this.rectangle_0.Y, (float)this.rectangle_0.X + (float)this.decimal_0 + 7f, (float)(base.Height - 1), this.float_0);
            GraphicsPath path2 = Helper.smethod_0((float)this.rectangle_0.X, (float)this.rectangle_0.Y, (float)(this.rectangle_0.X + this.rectangle_2.Width + 7), (float)(base.Height - 1), this.float_0);
            if (this.bool_6 && this.color_0 != Color.Empty && this.color_2 != Color.Empty && this.color_1 != Color.Empty)
            {
                using (GraphicsPath graphicsPath = Helper.smethod_0(0f, 0f, (float)(base.Width - 1), (float)(base.Height - 1), this.float_0))
                {
                    paintEventArgs_0.Graphics.FillPath(new SolidBrush(this.color_2), graphicsPath);
                    if (this.bool_1)
                    {
                        paintEventArgs_0.Graphics.FillPath(new SolidBrush(this.color_1), path);
                    }
                    else
                    {
                        paintEventArgs_0.Graphics.FillPath(new SolidBrush(this.color_1), path2);
                    }
                    paintEventArgs_0.Graphics.DrawPath(pen2, graphicsPath);
                    paintEventArgs_0.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    goto IL_433;
                }
            }
            using (GraphicsPath graphicsPath2 = Helper.smethod_0(0f, 0f, (float)(base.Width - 1), (float)(base.Height - 1), this.float_0))
            {
                paintEventArgs_0.Graphics.FillPath(new SolidBrush(color_6), graphicsPath2);
                if (this.bool_1)
                {
                    paintEventArgs_0.Graphics.FillPath(new SolidBrush(this.Color_4), path);
                }
                else
                {
                    paintEventArgs_0.Graphics.FillPath(new SolidBrush(this.Color_4), path2);
                }
                paintEventArgs_0.Graphics.DrawPath(pen, graphicsPath2);
                paintEventArgs_0.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            }
        IL_433:
            if (this.Boolean_0)
            {
                Font font = new Font(this.Font, FontStyle.Regular);
                paintEventArgs_0.Graphics.DrawString(this.Value.ToString(), font, new SolidBrush(this.ForeColor), new Rectangle(0, 0, base.Width, base.Height + 2), new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });
            }
            if (this.ShowText == true)
            {
                Font font2 = new Font(this.Font, FontStyle.Regular);
                paintEventArgs_0.Graphics.DrawString(this.Text, font2, new SolidBrush(this.ForeColor), new Rectangle(0, 0, base.Width, base.Height + 2), new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                });
            }
        }


        public bool ShowText { get; set; }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                base.Capture = true;
                ScrollEventHandler scrollEventHandler = this.scrollEventHandler_0;
                if (scrollEventHandler != null)
                {
                    scrollEventHandler(this, new ScrollEventArgs(ScrollEventType.ThumbTrack, this.int_2));
                }
                EventHandler eventHandler = this.eventHandler_0;
                if (eventHandler != null)
                {
                    eventHandler(this, new EventArgs());
                }
                this.OnMouseMove(e);
            }
            base.Invalidate();
        }


        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (base.Capture & e.Button == MouseButtons.Left)
            {
                ScrollEventType type = ScrollEventType.ThumbPosition;
                int num = e.Location.X;
                int num2 = 8;
                num -= num2;
                this.int_2 = this.int_3 + (num - this.int_0 - 8) * (this.int_4 - this.int_3) / (base.ClientRectangle.Width - this.int_0 - this.int_1 - 16);
                if (this.int_2 <= this.int_3)
                {
                    this.int_2 = this.int_3;
                    type = ScrollEventType.First;
                }
                else if (this.int_2 >= this.int_4)
                {
                    this.int_2 = this.int_4;
                    type = ScrollEventType.Last;
                }
                ScrollEventHandler scrollEventHandler = this.scrollEventHandler_0;
                if (scrollEventHandler != null)
                {
                    scrollEventHandler(this, new ScrollEventArgs(type, this.int_2));
                }
                EventHandler eventHandler = this.eventHandler_0;
                if (eventHandler != null)
                {
                    eventHandler(this, new EventArgs());
                }
            }
            base.Invalidate();
        }


        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.bool_6 = true;
            base.Invalidate();
        }


        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.bool_6 = false;
            base.Invalidate();
        }


        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            base.Capture = false;
            ScrollEventHandler scrollEventHandler = this.scrollEventHandler_0;
            if (scrollEventHandler != null)
            {
                scrollEventHandler(this, new ScrollEventArgs(ScrollEventType.EndScroll, this.int_2));
            }
            EventHandler eventHandler = this.eventHandler_0;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
            base.Invalidate();
        }


        [CompilerGenerated]
        private void method_3()
        {
            this.method_1();
        }


        [CompilerGenerated]
        private EventHandler eventHandler_0;


        [CompilerGenerated]
        private ScrollEventHandler scrollEventHandler_0;

        private Rectangle rectangle_0;
        private Rectangle rectangle_1;
        private Rectangle rectangle_2;
        private readonly int int_0;
        private readonly int int_1;
        private Rectangle rectangle_3;
        private Color color_0;
        private Color color_1;
        private Color color_2;
        private bool bool_0;
        private float float_0 = 1f;
        private int int_2 = 10;
        private int int_3;
        private int int_4 = 100;
        private float float_1 = 1f;
        private bool bool_1 = true;
        private Color color_3 = Color.Black;
        private Color color_4 = Color.FromArgb(21, 56, 152);
        private Color color_5 = Color.FromArgb(0, 0, 0);
        private decimal decimal_0 = 1m;
        private bool bool_2 = true;
        public static bool bool_3;
        private int int_5;
        private int int_6 = 8;
        [CompilerGenerated]
        private bool bool_4;
        public static bool bool_5;
        private bool bool_6;
    }
}
