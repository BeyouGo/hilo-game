
export interface AuthTokens {
    accessToken: string;   // JWT
    refreshToken?: string;
    expiresIn?: number;
}

export interface LoginDataModel {
    username: string;
    password: string;
}

export interface RegisterDto {
    username: string;
    password: string;
}
