# User Management API - Back-End Development with .NET (Coursera - Microsoft)

This is a simple **User Management API** built using **ASP.NET Core** to demonstrate the use of **authentication** and **session management** in a back-end development project. This API provides the functionality to authenticate users through a login endpoint, retrieve a session key, and use it to access user-related data.

## **Project Overview**

This project is part of the **Back-End Development with .NET** course on Coursera from Microsoft. The API allows users to:
1. **Login** using their credentials (email and password).
2. Retrieve a **SessionKey** after successful login.
3. Use the **SessionKey** to make authenticated requests to the `/users` endpoint and access user data.

## **Features**

- **Login Endpoint (`/login`)**: Allows users to log in with their email and password.
- **Session Management**: After a successful login, the API returns a session key that the user must include in subsequent requests to authenticate and retrieve user data.
- **User Endpoint (`/users`)**: A protected endpoint that returns the list of users, accessible only with a valid session key.

## **API Endpoints**

### **1. Login**
- **Endpoint**: `/login`
- **Method**: `POST`
- **Description**: Logs in the user with email and password, and returns a session key.

#### **Request**
```bash
curl -X POST https://your-api-url/login \
-H "Content-Type: application/json" \
-d '{"email": "alice@example.com", "password": "password123"}'


### Authenticated Request
After you have the **SessionKey**, you can make an authenticated request to `/users`.

```bash
curl -X GET https://your-api-url/users \
-H "SessionKey: your-session-key"