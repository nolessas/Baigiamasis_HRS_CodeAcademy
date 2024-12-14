class ApiConfig {
    static BASE_URL = '/api';
    
    static ENDPOINTS = {
        AUTH: {
            LOGIN: `${this.BASE_URL}/Auth/login`,
            REGISTER: `${this.BASE_URL}/Auth/register`,
            REFRESH: `${this.BASE_URL}/Auth/refresh-token`
        },
        HUMAN_INFO: {
            BASE: `${this.BASE_URL}/HumanInformation`,
            GET_BY_USER: (userId) => `${this.BASE_URL}/HumanInformation/user/${userId}`,
            UPDATE_PROFILE_PICTURE: (id) => `${this.BASE_URL}/HumanInformation/${id}/profile-picture`,
            UPDATE_FIELD: (id, field) => `${this.BASE_URL}/HumanInformation/${id}/${field}`
        },
        ADMIN: {
            USERS: `${this.BASE_URL}/Admin/users`,
            DELETE_USER: (userId) => `${this.BASE_URL}/Admin/users/${userId}`
        }
    };
} 