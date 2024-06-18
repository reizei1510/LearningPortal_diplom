import { getToken, saveTokens, deleteTokens } from './token.js';

async function showLogin() {
    const token = await getToken();
    if (token) {
        const loginFalse = document.querySelector(`#login-false`);
        loginFalse.hidden = true;

        const login = sessionStorage.getItem(`login`);
        const welcomeText = document.querySelector(`#welcome`);
        welcomeText.innerHTML = `Добро пожаловать, ${login}. `;
    }
    else {
        const loginTrue = document.querySelector(`#login-true`);
        loginTrue.hidden = true;
    }
}

document.querySelector(`#btn-in`).addEventListener(`click`, async () => {
    const login = document.querySelector(`#login`).value;
    const password = document.querySelector(`#password`).value;
    if (login.length == 0) {
        const error = document.querySelector(`#signin-error`);
        error.style.color = `#F00`;
        error.textContent = ` Заполните логин`;
    }
    else if (password.length < 6) {
        const error = document.querySelector(`#signin-error`);
        error.style.color = `#F00`;
        error.textContent = ` Пароль должен содержать не менее 6 символов`;
    }
    else {
        const response = await fetch(`/api/login/`, {
            method: `POST`,
            headers: {
                "Content-Type": `application/json`
            },
            body: JSON.stringify({ login, password })
        });
        if (response.ok === true) {
            const tokens = await response.json();
            saveTokens(tokens.token, tokens.refreshToken);
            sessionStorage.setItem(`login`, document.querySelector(`#login`).value);
            window.location.href = `/`;
        }
        else {
            let error = document.querySelector(`#signin-error`);
            error.style.color = `#F00`;
            error.textContent = `Неправильный пароль`;
        }
    }
});

document.querySelector(`#btn-out`).addEventListener(`click`, async () => {
    deleteTokens();
    window.location.href = `/`;
});

showLogin();