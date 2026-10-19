package com.mirror;

import android.content.Context;
import android.provider.Settings;
import android.util.Log;

import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.Map;

/**
 * FearAuth - Effortless Authentication for Android
 * Simplified version for custom backend integration.
 */
public class FearAuth {
    private final String appname;
    private final String ownerid;
    private final String version;
    private final String url;
    private final Context context;
    private String sessionid;

    public FearAuth(String appname, String ownerid, String version, String url, Context context) {
        this.appname = appname;
        this.ownerid = ownerid;
        this.version = version;
        this.url = url;
        this.context = context;
        this.sessionid = null;
    }

    public void init() throws Exception {
        Map<String, String> postData = new HashMap<>();
        postData.put("type", "init");
        postData.put("name", appname);
        postData.put("ownerid", ownerid);
        postData.put("ver", version);

        JSONObject responseJSON = makeApiCall(postData);

        if (!responseJSON.optBoolean("success")) {
            throw new Exception(responseJSON.optString("message", "Initialization failed"));
        }
        
        this.sessionid = responseJSON.optString("sessionid", null);
        if (this.sessionid == null || this.sessionid.isEmpty()) {
            throw new Exception("Session ID missing in backend response");
        }
    }

    public UserData login(String username, String password) throws Exception {
        checkInit();
        
        Map<String, String> postData = createBaseMap("login");
        postData.put("username", username);
        postData.put("pass", password);
        postData.put("hwid", getHwid());

        JSONObject responseJSON = makeApiCall(postData);
        if (!responseJSON.optBoolean("success")) {
            throw new Exception(responseJSON.optString("message", "Login failed"));
        }
        return new UserData(context, responseJSON);
    }

    public UserData license(String key) throws Exception {
        checkInit();
        
        Map<String, String> postData = createBaseMap("license");
        postData.put("key", key);
        postData.put("hwid", getHwid());

        JSONObject responseJSON = makeApiCall(postData);
        if (!responseJSON.optBoolean("success")) {
            throw new Exception(responseJSON.optString("message", "License failed"));
        }
        return new UserData(context, responseJSON);
    }

    public void upgrade(String username, String key) throws Exception {
        checkInit();
        
        Map<String, String> postData = createBaseMap("upgrade");
        postData.put("username", username);
        postData.put("key", key);

        JSONObject responseJSON = makeApiCall(postData);
        if (!responseJSON.optBoolean("success")) {
            throw new Exception(responseJSON.optString("message", "Upgrade failed"));
        }
    }

    public boolean register(String username, String password, String key) throws Exception {
        checkInit();
        
        Map<String, String> postData = createBaseMap("register");
        postData.put("username", username);
        postData.put("pass", password);
        postData.put("key", key);
        postData.put("hwid", getHwid());

        JSONObject responseJSON = makeApiCall(postData);
        if (!responseJSON.optBoolean("success")) {
            throw new Exception(responseJSON.optString("message", "Registration failed"));
        }
        return true;
    }

    private synchronized void checkInit() throws Exception {
        if (sessionid == null || sessionid.isEmpty()) {
            init(); // Auto-init if called before finish
        }
    }

    private Map<String, String> createBaseMap(String type) {
        Map<String, String> map = new HashMap<>();
        map.put("type", type);
        map.put("name", appname);
        map.put("ownerid", ownerid);
        map.put("sessionid", sessionid);
        return map;
    }

    private String getHwid() {
        try {
            return Settings.Secure.getString(context.getContentResolver(), Settings.Secure.ANDROID_ID);
        } catch (Exception e) {
            return "UNKNOWN-HWID";
        }
    }

    private JSONObject makeApiCall(Map<String, String> postData) throws Exception {
        URL apiUrl = new URL(url);
        HttpURLConnection conn = (HttpURLConnection) apiUrl.openConnection();
        conn.setRequestMethod("POST");
        conn.setDoOutput(true);
        conn.setConnectTimeout(60000); // 60s for Render wake up
        conn.setReadTimeout(60000);
        conn.setRequestProperty("Content-Type", "application/x-www-form-urlencoded");

        StringBuilder postDataString = new StringBuilder();
        for (Map.Entry<String, String> entry : postData.entrySet()) {
            if (postDataString.length() != 0) postDataString.append('&');
            postDataString.append(URLEncoder.encode(entry.getKey(), "UTF-8"));
            postDataString.append('=');
            postDataString.append(URLEncoder.encode(entry.getValue(), "UTF-8"));
        }

        byte[] postDataBytes = postDataString.toString().getBytes(StandardCharsets.UTF_8);
        
        try (OutputStream os = conn.getOutputStream()) {
            os.write(postDataBytes);
        }

        int responseCode = conn.getResponseCode();
        BufferedReader in = new BufferedReader(new InputStreamReader(
                responseCode == 200 ? conn.getInputStream() : conn.getErrorStream()
        ));
        
        StringBuilder response = new StringBuilder();
        String inputLine;
        while ((inputLine = in.readLine()) != null) {
            response.append(inputLine);
        }
        in.close();

        return new JSONObject(response.toString());
    }

    public String getSessionId() {
        return sessionid;
    }
}
