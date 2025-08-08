// Dashboard JavaScript Functions

$(document).ready(function () {
    // Initialize charts
    initializeRevenueChart();
    initializeProductsChart();
    initializeCategoryChart();

    // Load dynamic content
    loadRecentOrders();
    loadNotifications();

    // Handle time range change
    $('#timeRange').change(function () {
        var days = $(this).val();
        updateRevenueChart(days);
        showLoadingToast();
    });

    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Auto-refresh every 5 minutes
    setInterval(function () {
        loadNotifications();
        loadRecentOrders();
    }, 300000);

    console.log('Dashboard initialized successfully');
});

// Revenue Chart
function initializeRevenueChart() {
    $.get(window.dashboardData.urls.getSalesData, function (data) {
        console.log({data});
        if (data.error) {
            showErrorToast('Failed to load revenue data');
            return;
        }

        var ctx = document.getElementById('revenueChart').getContext('2d');

        // Create gradient
        var gradient = ctx.createLinearGradient(0, 0, 0, 400);
        gradient.addColorStop(0, 'rgba(54, 162, 235, 0.4)');
        gradient.addColorStop(1, 'rgba(54, 162, 235, 0.05)');

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.map(d => formatDate(new Date(d.date))),
                datasets: [{
                    label: 'Revenue ($)',
                    data: data.map(d => d.sales),
                    borderColor: '#36A2EB',
                    backgroundColor: gradient,
                    borderWidth: 3,
                    tension: 0.4,
                    fill: true,
                    pointBackgroundColor: '#36A2EB',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 5,
                    pointHoverRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        borderColor: '#36A2EB',
                        borderWidth: 1,
                        cornerRadius: 8,
                        displayColors: false,
                        callbacks: {
                            title: function (context) {
                                return 'Date: ' + context[0].label;
                            },
                            label: function (context) {
                                return 'Revenue: $' + context.parsed.y.toLocaleString();
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            font: { size: 12 },
                            color: '#6c757d'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0, 0, 0, 0.05)'
                        },
                        ticks: {
                            font: { size: 12 },
                            color: '#6c757d',
                            callback: function (value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                },
                animation: {
                    duration: 2000,
                    easing: 'easeInOutQuart'
                }
            }
        });
    }).fail(function () {
        showChartError('revenueChart', 'Failed to load revenue data');
    });
}

// Products Chart
function initializeProductsChart() {
    var productData = window.dashboardData.popularProducts;

    if (!productData || productData.length === 0) {
        showChartError('productsChart', 'No product data available');
        return;
    }

    var ctx = document.getElementById('productsChart').getContext('2d');

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: productData.map(p => truncateText(p.productName, 15)),
            datasets: [{
                label: 'Units Sold',
                data: productData.map(p => p.unitsSold),
                backgroundColor: [
                    'rgba(255, 99, 132, 0.8)',
                    'rgba(54, 162, 235, 0.8)',
                    'rgba(255, 205, 86, 0.8)',
                    'rgba(75, 192, 192, 0.8)',
                    'rgba(153, 102, 255, 0.8)'
                ],
                borderColor: [
                    'rgba(255, 99, 132, 1)',
                    'rgba(54, 162, 235, 1)',
                    'rgba(255, 205, 86, 1)',
                    'rgba(75, 192, 192, 1)',
                    'rgba(153, 102, 255, 1)'
                ],
                borderWidth: 2,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    cornerRadius: 8,
                    callbacks: {
                        title: function (context) {
                            return productData[context[0].dataIndex].productName;
                        },
                        label: function (context) {
                            return 'Units Sold: ' + context.parsed.x.toLocaleString();
                        },
                        afterLabel: function (context) {
                            var revenue = productData[context.dataIndex].revenue;
                            return 'Revenue: $' + revenue.toLocaleString();
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    },
                    ticks: {
                        font: { size: 11 },
                        color: '#6c757d'
                    }
                },
                y: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        font: { size: 11 },
                        color: '#6c757d'
                    }
                }
            },
            animation: {
                duration: 1500,
                easing: 'easeOutBounce'
            }
        }
    });
}

// Category Chart
function initializeCategoryChart() {
    $.get(window.dashboardData.urls.getCategoryData, function (data) {
        if (data.error) {
            showErrorToast('Failed to load category data');
            return;
        }

        var ctx = document.getElementById('categoryChart').getContext('2d');

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: data.map(c => c.category),
                datasets: [{
                    data: data.map(c => c.sales),
                    backgroundColor: [
                        '#FF6384', '#36A2EB', '#FFCE56',
                        '#4BC0C0', '#9966FF', '#FF9F40'
                    ],
                    hoverBackgroundColor: [
                        '#FF6384', '#36A2EB', '#FFCE56',
                        '#4BC0C0', '#9966FF', '#FF9F40'
                    ],
                    borderWidth: 0,
                    hoverBorderWidth: 3,
                    hoverBorderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            padding: 20,
                            font: { size: 12 }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        titleColor: '#fff',
                        bodyColor: '#fff',
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                var label = context.label || '';
                                var value = context.parsed;
                                var total = context.dataset.data.reduce((a, b) => a + b, 0);
                                var percentage = ((value / total) * 100).toFixed(1);
                                return label + ': $' + value.toLocaleString() + ' (' + percentage + '%)';
                            }
                        }
                    }
                },
                animation: {
                    animateRotate: true,
                    duration: 2000
                },
                cutout: '60%'
            }
        });
    }).fail(function () {
        showChartError('categoryChart', 'Failed to load category data');
    });
}

// Load recent orders
function loadRecentOrders() {
    $.get(window.dashboardData.urls.getRecentOrders, function (data) {
        if (data.error) {
            $('#recentOrdersTable').html('<tr><td colspan="5" class="text-center text-danger">Failed to load orders</td></tr>');
            return;
        }

        var html = '';
        if (data.length === 0) {
            html = '<tr><td colspan="5" class="text-center text-muted">No recent orders</td></tr>';
        } else {
            data.forEach(function (order) {
                html += `
                    <tr>
                        <td><strong>#${order.orderId}</strong></td>
                        <td>${order.customerName || 'N/A'}</td>
                        <td><strong>$${order.totalAmount.toLocaleString()}</strong></td>
                        <td><span class="badge ${order.statusBadgeClass}">${order.orderStatus}</span></td>
                        <td><small>${formatDate(new Date(order.orderDate))}</small></td>
                    </tr>
                `;
            });
        }

        $('#recentOrdersTable').html(html);
    }).fail(function () {
        $('#recentOrdersTable').html('<tr><td colspan="5" class="text-center text-danger">Failed to load orders</td></tr>');
    });
}

// =================== Load Notifications ===================
function loadNotifications() {
    $.get(window.dashboardData.urls.getDashboardNotifications, function (data) {
        if (data.error) {
            $('#notificationsList').html('<li class="list-group-item text-danger">Failed to load notifications</li>');
            return;
        }

        var html = '';
        if (data.length === 0) {
            html = '<li class="list-group-item text-muted">No notifications</li>';
        } else {
            data.forEach(function (n) {
                html += `
                    <li class="list-group-item d-flex justify-content-between align-items-center">
                        <div>
                            <strong>${n.title}</strong>
                            <p class="mb-0 small text-muted">${n.message}</p>
                        </div>
                        <span class="badge badge-${getNotificationBadge(n.type)}">${formatDate(new Date(n.createdAt))}</span>
                    </li>
                `;
            });
        }
        $('#notificationsList').html(html);
    }).fail(function () {
        $('#notificationsList').html('<li class="list-group-item text-danger">Failed to load notifications</li>');
    });
}

// =================== Helper Functions ===================
function showLoadingToast() {
    toastr.info('Loading data...');
}

function showErrorToast(msg) {
    toastr.error(msg);
}

function showChartError(chartId, msg) {
    var canvas = document.getElementById(chartId);
    if (canvas) {
        var ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.font = "16px Arial";
        ctx.fillStyle = "red";
        ctx.fillText(msg, 10, 50);
    }
}

function formatDate(date) {
    return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
}

function truncateText(text, length) {
    return text.length > length ? text.substring(0, length) + '…' : text;
}

function getNotificationBadge(type) {
    switch (type) {
        case 'success': return 'success';
        case 'warning': return 'warning';
        case 'info': return 'info';
        case 'error': return 'danger';
        default: return 'secondary';
    }
}