function createGenderChart(ctx) {
    return new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ["Homme", "Femme"],
            datasets: [{
                label: 'Genres',
                data: [0, 0],
                backgroundColor: [
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(255, 99, 132, 0.8)'
                ],
                borderColor: [
                    'rgba(54, 162, 235, 1)',
                    'rgba(255,99,132,1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
    });
}

function createEmotionChart(ctx) {
    return new Chart(ctx, {
        type: 'bar',
        data: {
            labels: ["colère", "mépris", "dégoût", "peur", "joie", "neutre", "tristesse", "surprise"],
            datasets: [{
                label: '# de visiteurs',
                data: [0, 0, 0, 0, 0, 0, 0, 0],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.8)',
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(255, 206, 86, 0.8)',
                    'rgba(75, 192, 192, 0.8)',
                    'rgba(153, 102, 255, 0.8)',
                    'rgba(255, 159, 64, 0.8)',
                    'rgba(32, 192, 255, 0.8)',
                    'rgba(255, 27, 164, 0.8)'
                ],
                borderColor: [
                    'rgba(255,99,132,1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)',
                    'rgba(32, 192, 255, 1)',
                    'rgba(255, 27, 164, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
    });
}

let hairLabels = [];
let hairValues = [];

function createHairColorChart(ctx) {
    return new Chart(ctx, {
        type: 'bar',
        data: {
            labels: hairLabels,
            datasets: [{
                label: '# de visiteurs',
                data: hairValues,
                backgroundColor: [
                    'rgba(255, 99, 132, 0.8)',
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(255, 206, 86, 0.8)',
                    'rgba(75, 192, 192, 0.8)',
                    'rgba(153, 102, 255, 0.8)',
                    'rgba(255, 159, 64, 0.8)',
                    'rgba(32, 192, 255, 0.8)',
                    'rgba(255, 27, 164, 0.8)'
                ],
                borderColor: [
                    'rgba(255,99,132,1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 206, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)',
                    'rgba(255, 159, 64, 1)',
                    'rgba(32, 192, 255, 1)',
                    'rgba(255, 27, 164, 1)'
                ],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
    });
}

$(document).ready(function () {

    var genderChart = createGenderChart(document.getElementById("gender-chart").getContext('2d'));
    var emotionChart = createEmotionChart(document.getElementById("emotion-chart").getContext('2d'));
    var hairChart = createHairColorChart(document.getElementById("hair-chart").getContext('2d'));

    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/notify")
        .build();

    connection.on("NotifyNewPerson", function (face) {
        var message = "A ";

        message += face.Age;

        if (face.Gender === 1) {
            message += " years old dude ";
        }
        else {
            message += " years old lady ";
        }

        message += "is here!";

        //alert(message);
    });

    connection.on("NotifyStatChanged", function (stats) {
        genderChart.data.datasets[0].data = [stats.CountMale, stats.CountFemale];
        genderChart.update();

        emotionChart.data.datasets[0].data = [
            stats.CountAnger,
            stats.CountContempt,
            stats.CountDisgust,
            stats.CountFear,
            stats.CountHappiness,
            stats.CountNeutral,
            stats.CountSadness,
            stats.CountSurprise
        ];
        emotionChart.update();

        document.getElementById("avgAge").innerText = stats.AvgAge.toFixed(1);

        var hairColors = JSON.parse(stats.HairColor);

        //Cleaning
        var currentLength = hairLabels.length;
        for (var i = 0; i < currentLength; i++) {
            hairLabels.pop();
            hairValues.pop();
        }

        for (var i in hairColors) {
            hairLabels.push(i);
            hairValues.push(hairColors[i]);
        }

        hairChart.update();
    });

    connection.start().catch(function (err) {
        return console.error(err.toString());
    });
});