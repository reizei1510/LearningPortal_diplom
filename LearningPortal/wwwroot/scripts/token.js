export async function getToken() {
    let token = sessionStorage.getItem(`token`);

    if (token && isTokenExpiring(token, 10)) {
        let refreshToken = sessionStorage.getItem(`refreshToken`);
        token = await refreshTokens(token, refreshToken);
    }

    return token;
}

export function saveTokens(token, refreshToken) {
    sessionStorage.setItem(`token`, token);
    sessionStorage.setItem(`refreshToken`, refreshToken);
}

export function deleteTokens() {
    sessionStorage.removeItem(`login`);
    sessionStorage.removeItem(`token`);
    sessionStorage.removeItem(`refreshToken`);
}

function isTokenExpiring(token, threshold) {
    const tokenPayload = JSON.parse(atob(token.split('.')[1]));
    const expirationTime = tokenPayload.exp;
    const currentTime = Math.floor(Date.now() / 1000);

    return expirationTime - currentTime < threshold;
}

async function refreshTokens(token, refreshToken) {
    response = await fetch(`/api/refresh-token`, {
        method: `POST`,
        headers: {
            "Content-Type": `application/json`,
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify({ token, refreshToken })
    });
    if (response.ok) {
        const newTokens = response.json();

        if (newTokens) {
            const token = newTokens.token;
            const refreshToken = newTokens.refreshToken;
            saveTokens(token, refreshToken);

            return token;
        }
        else {
            console.log(`Invalid token`);
            deleteTokens();

            return null;
        }
    }
}