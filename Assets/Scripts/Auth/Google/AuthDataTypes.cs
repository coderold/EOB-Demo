using System;

[Serializable]
public class GoogleLoginRequest
{
    public string idToken;
}

[Serializable]
public class GoogleLoginResponse
{
    public string accessToken;
    public string refreshToken;
    public string accessTokenExpiration;
    public string refreshTokenExpiration;
    public UserData user;
}

[Serializable]
public class UserData
{
    public string id;
    public string userName;
    public string email;
    public string createdAt;
}