#pragma once
#include <iostream>
#include <string>
#include <vector>
#include <windows.h>
#include <wininet.h>

#pragma comment(lib, "wininet.lib")
#pragma comment(lib, "advapi32.lib")

namespace FearAuth {

    class api {
    public:
        std::string name, ownerid, secret, version, url;

        struct data_class {
            bool success;
            std::string message;
            std::string sessionid;
        } data;

        api(std::string name, std::string ownerid, std::string secret, std::string version, std::string url) :
            name(name), ownerid(ownerid), secret(secret), version(version), url(url) {
            data.success = false;
        }

        void init() {
            std::string post_data = "type=init&name=" + name + "&ownerid=" + ownerid + "&ver=" + version;
            std::string response = make_request(post_data);

            if (response.find("\"success\":true") != std::string::npos) {
                data.success = true;
                data.message = "Initialized";
                data.sessionid = parse_json_value(response, "sessionid");
            }
            else {
                data.success = false;
                data.message = parse_json_value(response, "message");
            }
        }

        void login(std::string username, std::string password) {
            check_init();
            std::string post_data = "type=login&name=" + name + "&ownerid=" + ownerid + "&sessionid=" + data.sessionid + "&username=" + username + "&pass=" + password + "&hwid=" + get_hwid();
            std::string response = make_request(post_data);

            if (response.find("\"success\":true") != std::string::npos) {
                data.success = true;
                data.message = "Logged in!";
            }
            else {
                data.success = false;
                data.message = parse_json_value(response, "message");
            }
        }

        void regstr(std::string username, std::string password, std::string key) {
            check_init();
            std::string post_data = "type=register&name=" + name + "&ownerid=" + ownerid + "&sessionid=" + data.sessionid + "&username=" + username + "&pass=" + password + "&key=" + key + "&hwid=" + get_hwid();
            std::string response = make_request(post_data);

            if (response.find("\"success\":true") != std::string::npos) {
                data.success = true;
                data.message = "Registered!";
            }
            else {
                data.success = false;
                data.message = parse_json_value(response, "message");
            }
        }

        void license(std::string key) {
            check_init();
            std::string post_data = "type=license&name=" + name + "&ownerid=" + ownerid + "&sessionid=" + data.sessionid + "&key=" + key + "&hwid=" + get_hwid();
            std::string response = make_request(post_data);

            if (response.find("\"success\":true") != std::string::npos) {
                data.success = true;
                data.message = "License activated!";
            }
            else {
                data.success = false;
                data.message = parse_json_value(response, "message");
            }
        }

        void upgrade(std::string username, std::string key) {
            check_init();
            std::string post_data = "type=upgrade&name=" + name + "&ownerid=" + ownerid + "&sessionid=" + data.sessionid + "&username=" + username + "&key=" + key;
            std::string response = make_request(post_data);

            if (response.find("\"success\":true") != std::string::npos) {
                data.success = true;
                data.message = "Upgraded successfully!";
            }
            else {
                data.success = false;
                data.message = parse_json_value(response, "message");
            }
        }

    private:
        void check_init() {
            if (data.sessionid.empty()) {
                init();
            }
        }

        std::string get_hwid() {
            HW_PROFILE_INFOA hwProfileInfo;
            if (GetCurrentHwProfileA(&hwProfileInfo))
                return hwProfileInfo.szHwProfileGuid;
            return "UNKNOWN-HWID";
        }

        std::string make_request(std::string data) {
            HINTERNET hInternet = InternetOpenA("FearAuth", INTERNET_OPEN_TYPE_DIRECT, NULL, NULL, 0);
            if (!hInternet) return "";

            // URL format expected: http://host/path or https://host/path
            std::string host = url;
            size_t start = host.find("//") + 2;
            host = host.substr(start);
            size_t slash = host.find("/");
            std::string path = host.substr(slash);
            host = host.substr(0, slash);

            HINTERNET hConnect = InternetConnectA(hInternet, host.c_str(), INTERNET_DEFAULT_HTTPS_PORT, NULL, NULL, INTERNET_SERVICE_HTTP, 0, 0);
            if (!hConnect) { InternetCloseHandle(hInternet); return ""; }

            HINTERNET hRequest = HttpOpenRequestA(hConnect, "POST", path.c_str(), NULL, NULL, NULL, INTERNET_FLAG_SECURE | INTERNET_FLAG_RELOAD, 0);
            if (!hRequest) { InternetCloseHandle(hConnect); InternetCloseHandle(hInternet); return ""; }

            std::string headers = "Content-Type: application/x-www-form-urlencoded";

            DWORD timeout = 60000;
            InternetSetOptionA(hRequest, INTERNET_OPTION_SEND_TIMEOUT, &timeout, sizeof(timeout));
            InternetSetOptionA(hRequest, INTERNET_OPTION_RECEIVE_TIMEOUT, &timeout, sizeof(timeout));
            InternetSetOptionA(hRequest, INTERNET_OPTION_CONNECT_TIMEOUT, &timeout, sizeof(timeout));

            if (!HttpSendRequestA(hRequest, headers.c_str(), (DWORD)headers.length(), (LPVOID)data.c_str(), (DWORD)data.length())) {
                InternetCloseHandle(hRequest); InternetCloseHandle(hConnect); InternetCloseHandle(hInternet);
                return "{\"success\":false,\"message\":\"Connection failed (Server cold start?)\"}";
            }

            std::string response;
            char buffer[4096];
            DWORD bytesRead = 0;
            while (InternetReadFile(hRequest, buffer, (DWORD)sizeof(buffer), &bytesRead) && bytesRead > 0) {
                response.append(buffer, bytesRead);
            }

            InternetCloseHandle(hRequest);
            InternetCloseHandle(hConnect);
            InternetCloseHandle(hInternet);
            return response;
        }

        std::string parse_json_value(std::string json, std::string key) {
            size_t pos = json.find("\"" + key + "\":\"");
            if (pos == std::string::npos) return "";
            pos += key.length() + 4;
            size_t end = json.find("\"", pos);
            if (end == std::string::npos) return "";
            return json.substr(pos, end - pos);
        }
    };
}
