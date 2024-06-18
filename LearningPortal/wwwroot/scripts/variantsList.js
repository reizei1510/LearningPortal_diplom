import { getToken } from './token.js';

async function getVariants() {
    const token = await getToken();

    const headers = {};
    if (token) {
        headers[`Authorization`] = `Bearer ${token}`;
    }
    headers[`Accept`] = `application/json`;

    const response = await fetch(`/api/check-solved-variants/`, {
        method: `GET`,
        headers
    });
    if (response.ok === true) {
        const variantsCount = await response.json();

        for (let i = 1; i <= variantsCount; i++) {
            const variantsContainer = document.querySelector(`#variants-container`);
            const variant = document.createElement(`a`);
            variant.innerHTML = `<br />Вариант ${i}<br />`;
            variant.href = `/results/${i}`;
            variantsContainer.appendChild(variant);
        }
    }
}

getVariants();