import { loadTasks } from './tasks.js';
import { getToken } from './token.js';

document.querySelector(`#btn-save`).addEventListener(`click`, async () => {
    const answers = [];

    document.querySelectorAll(`.task`).forEach(taskDiv => {
        const taskAnswer = [];
        taskDiv.querySelectorAll(`.answer-input`).forEach(input => {
            taskAnswer.push(input.value.trim());
        });
        answers.push(taskAnswer.join(` `));
    });

    const token = await getToken();

    const headers = {};
    if (token) {
        headers[`Authorization`] = `Bearer ${token}`;
    }
    headers[`Content-Type`] = `application/json`;

    const response = await fetch(`/api/save-results/`, {
        method: `POST`,
        headers,
        body: JSON.stringify(answers)
    });
    if (response.ok) {
        const num = document.querySelector(`#variant-num`).innerHTML;
        window.location.href = `/results/${num}`;
    }
    else {
        const error = document.getElementById(`#send-error`);
        error.innerHTML = ` Ошибка отправки данных`;
    }
});

loadTasks(0, false);