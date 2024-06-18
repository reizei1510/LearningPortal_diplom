import { loadTasks } from './tasks.js';
import { getToken } from './token.js';

async function loadResults() {
    const variantNum = window.location.href[window.location.href.length - 1];
    await loadTasks(variantNum, true);

    const token = await getToken();

    const headers = {};
    if (token) {
        headers[`Authorization`] = `Bearer ${token}`;
    }
    headers[`Accept`] = `application/json`;

    const response = await fetch(`/api/get-results/${variantNum}`, {
        method: `GET`,
        headers
    });
    if (response.ok === true) {
        const answers = await response.json();
        let i = 0;
        answers.forEach(answer => {
            const result = document.querySelector(`#result${i + 1}`);
            if (answer.userAnswer == answer.rightAnswer) {
                result.style.color = `#0F0`;
                result.innerHTML = `Верно. Ваш ответ: ${answer.userAnswer}.`;
            }
            else {
                result.style.color = `#F00`;
                result.innerHTML = `Неверно. Правильный ответ: ${answer.rightAnswer}. Ваш ответ: ${answer.userAnswer}.`;
            }
            i++;
        });
    }
}

document.querySelector(`#btn-next`).addEventListener(`click`, () => {
    window.location.href = `/`;
});

loadResults();