import { getToken } from './token.js';

function setToday() {
    var today = new Date();
    document.querySelector(`#date`).value = today.toISOString().substring(0, 10);
}

async function updateStatistic() {
    const date = document.querySelector(`#date`).value;

    const token = await getToken();

    const headers = {};
    if (token) {
        headers[`Authorization`] = `Bearer ${token}`;
    }
    headers[`Content-Type`] = `application/json`;

    const response = await fetch(`/api/statistic/`, {
        method: `POST`,
        headers,
        body: JSON.stringify({ date })
    });
    if (response.ok === true) {
        const statistic = await response.json();

        const avgGrade = countAvgGrade(statistic);
        document.querySelector(`#grade`).innerHTML = avgGrade;

        createTable(statistic);
        drawCanvas(statistic.solvingsCount);
        drawPie(statistic.solvingsCount, statistic.exercisesCount);
    }
}

function countAvgGrade(statistic) {
    const data = Array(statistic.exercisesCount).fill(0);
    Object.keys(statistic.solvingsCount).forEach(difficult => {
        statistic.solvingsCount[difficult].forEach((count, index) => {
            data[index] += count;
        });
    });

    let sumGrade = data.reduce((sum, value, index) => {
        if (index == 26 || index == 27) {
            return sum + value * 2;
        }
        return sum + value;
    }, 0);

    const avgGrade = sumGrade / data.length;
    return avgGrade.toFixed(2);
}

function createTable(statistic) {
    const vars = document.querySelector(`#variantsNum`);
    vars.innerHTML = `${statistic.variantsCount}`;

    const tableContainer = document.querySelector(`#table-container`);
    tableContainer.innerHTML = ``;

    const table = document.createElement(`table`);

    let row = document.createElement(`tr`);

    let cell = document.createElement(`th`);
    cell.innerHTML = `Номер задачи`;
    row.append(cell);

    cell = document.createElement(`th`);
    cell.setAttribute(`colspan`, `${Object.keys(statistic.solvingsCount).length}`);
    cell.innerHTML = `Количество решенных задач`;
    row.append(cell);

    table.append(row);

    row = document.createElement(`tr`);

    cell = document.createElement(`td`);
    cell.innerHTML = ``;
    row.append(cell);

    Object.entries(statistic.solvingsCount).forEach(([difficult, exercises]) => {
        cell = document.createElement(`td`);
        cell.innerHTML = `${difficult}-я сложность`;
        row.append(cell);
    });

    table.append(row);

    for (let i = 0; i < statistic.exercisesCount; i++) {
        row = document.createElement("tr");

        cell = document.createElement("td");
        cell.textContent = `${i + 1}`;
        row.append(cell);
        Object.entries(statistic.solvingsCount).forEach(([difficult, exercises]) => {
            cell = document.createElement("td");
            cell.textContent = `${exercises[i]}`;
            row.append(cell);
        });

        table.append(row);
    }

    tableContainer.appendChild(table);
}

function drawCanvas(solvingsCount) {
    const xData = Object.keys(solvingsCount);
    const yData = Object.values(solvingsCount).map(list => {
        return list.reduce((count, num) => {
            return count + num;
        }, 0);
    })

    const ctx = document.querySelector('#statistic-canvas').getContext('2d');
    const myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: xData,
            datasets: [
                {
                    label: 'Решенные задачи',
                    data: yData,
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                    borderColor: 'rgba(255, 99, 132, 1)',
                    borderWidth: 1,
                },
            ],
        },
        options: {
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
}

function drawPie(solvingsCount, exercisesCount) {
    const data = Array(exercisesCount).fill(0);
    Object.keys(solvingsCount).forEach(difficult => {
        for (let i = 0; i < data.length; i++) {
            data[i] += solvingsCount[difficult][i];
        }
    });

    const labels = Array.from({ length: 27 }, (_, index) => `Задание ` + (index + 1));

    const ctxPie = document.querySelector('#statistic-pie').getContext('2d');
    const myPieChart = new Chart(ctxPie, {
        type: 'pie',
        data: {
            labels: labels,
            datasets: [
                {
                    data: data,
                    backgroundColor: 'rgba(90, 99, 255, 0.2)',
                    hoverBackgroundColor: 'rgba(90, 99, 255, 0.4)'
                },
            ]
        },
        options: {
            plugins: {
                legend: {
                    display: false
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

setToday();
updateStatistic();
