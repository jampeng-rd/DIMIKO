document.addEventListener("DOMContentLoaded", function () {

    const data = window.dashboardData;

    if (!data) {
        return;
    }


    // 每日營收
    const revenueCanvas = document.getElementById("dailyRevenueChart");

    if (revenueCanvas) {

        new Chart(revenueCanvas, {
            type: "line",

            data: {
                labels: data.dailyRevenue.map(item => `${item.Day} 日`),

                datasets: [{
                    label: "營收",
                    data: data.dailyRevenue.map(item => item.Revenue),
                    borderWidth: 2,
                    tension: 0.3
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }


    // 每日訂單
    const ordersCanvas = document.getElementById("dailyOrdersChart");

    if (ordersCanvas) {

        new Chart(ordersCanvas, {
            type: "bar",

            data: {
                labels: data.dailyOrders.map(item => `${item.Day} 日`),

                datasets: [{
                    label: "訂單數",
                    data: data.dailyOrders.map(item => item.Count),
                    borderWidth: 1
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true,

                        ticks: {
                            precision: 0
                        }
                    }
                }
            }
        });
    }


    // 訂單狀態
    const statusCanvas = document.getElementById("orderStatusChart");

    if (statusCanvas) {

        new Chart(statusCanvas, {
            type: "doughnut",

            data: {
                labels: data.orderStatusBreakdown.map(item => item.Status),

                datasets: [{
                    data: data.orderStatusBreakdown.map(item => item.Count)
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        position: "bottom"
                    }
                }
            }
        });
    }


    // 商品分類 (用 Bar Chart)
    const categoryCanvas = document.getElementById("categoryProductChart");

    if (categoryCanvas) {

        new Chart(categoryCanvas, {
            type: "bar",

            data: {
                labels: data.productsPerCategory.map(item => item.Category),

                datasets: [{
                    label: "商品數量",

                    data: data.productsPerCategory.map(item => item.Count),

                    borderWidth: 1
                }]
            },

            options: {
                responsive: true,
                maintainAspectRatio: false,

                plugins: {
                    legend: {
                        display: false
                    }
                },

                scales: {
                    y: {
                        beginAtZero: true,

                        ticks: {
                            precision: 0
                        }
                    }
                }
            }
        });
    }


});
