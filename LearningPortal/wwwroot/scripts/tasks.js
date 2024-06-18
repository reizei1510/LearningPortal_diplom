import { getToken } from './token.js';

export async function loadTasks(variantNum, results) {
    const token = await getToken();

    const headers = {};
    if (token) {
        headers[`Authorization`] = `Bearer ${token}`;
    }
    headers[`Accept`] = `application/json`;

    const response = await fetch(`/api/get-variant/${variantNum}`, {
        method: `GET`,
        headers
    });
    if (response.ok) {
        const variant = await response.json();

        document.title += ` ${variant.num}`;

        const variantNum = document.querySelector(`#variant-num`);
        variantNum.append(variant.num);

        const variantLvl = document.querySelector(`#variant-lvl`);
        variantLvl.append(variant.lvl);

        const tasksContainer = document.querySelector(`#tasks-container`);
        variant.exercises.forEach(task => {
            const taskDiv = document.createElement(`div`);
            taskDiv.className = `task`;

            const taskText = document.createElement(`p`);
            taskText.innerHTML = `<br />Задача ${task.num}<br /><br />${task.text}<br /></br>`;
            taskDiv.append(taskText);

            task.files.forEach(file => {
                if (file.extension == `.doc` || file.extension == `.xlsx`) {
                    const taskFile = document.createElement(`a`);
                    taskFile.innerHTML = `Задача ${task.num}`;
                    if (task.files.length > 1) {
                        taskFile.innerHTML += `_${file.num}</br>`;
                    }
                    taskFile.href = `../../tasks/${file.path}`;
                    taskDiv.append(taskFile);
                }
                if (file.extension == `.png`) {
                    const taskFile = document.createElement(`img`);
                    taskFile.alt = `Задача ${task.num}_${file.num}`;
                    taskFile.src = `../../tasks/${file.path}`;
                    taskDiv.append(taskFile);
                }
            });

            if (results) {
                const result = document.createElement(`p`);
                result.id = `result${task.num}`;
                taskDiv.append(result);
            }
            else {
                const answerText = document.createElement(`p`);
                answerText.innerHTML = `Ответ:`;
                taskDiv.append(answerText);

                task.answer.split(`\n`).forEach(answerRow => {
                    if (answerRow != "") {
                        const inputRow = document.createElement(`div`);
                        answerRow.split(` `).forEach(answer => {
                            const answerInput = document.createElement(`input`);
                            answerInput.className = `answer-input`;
                            inputRow.append(answerInput);
                        });
                        taskDiv.append(inputRow);
                    }
                });
            }

            tasksContainer.append(taskDiv);
        });
    }
}