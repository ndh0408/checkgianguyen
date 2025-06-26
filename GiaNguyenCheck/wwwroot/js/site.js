// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Sidebar Toggle
$(document).ready(function () {
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });
});

// SignalR Connection
let dashboardHub;
let checkInHub;

// Initialize SignalR connections
function initializeSignalR() {
    // Dashboard Hub
    dashboardHub = new signalR.HubConnectionBuilder()
        .withUrl("/dashboardHub")
        .withAutomaticReconnect()
        .build();

    dashboardHub.on("UpdateDashboardStats", function (stats) {
        updateDashboardStats(stats);
    });

    dashboardHub.on("UpdateCheckInCount", function (count) {
        updateCheckInCount(count);
    });

    // Check-in Hub
    checkInHub = new signalR.HubConnectionBuilder()
        .withUrl("/checkInHub")
        .withAutomaticReconnect()
        .build();

    checkInHub.on("GuestCheckedIn", function (guestData) {
        showGuestCheckedIn(guestData);
    });

    checkInHub.on("GuestCheckedOut", function (guestData) {
        showGuestCheckedOut(guestData);
    });

    // Start connections
    dashboardHub.start().catch(function (err) {
        console.error("Dashboard Hub Error: ", err);
    });

    checkInHub.start().catch(function (err) {
        console.error("Check-in Hub Error: ", err);
    });
}

// Update Dashboard Statistics
function updateDashboardStats(stats) {
    if (stats.totalEvents !== undefined) {
        $('#totalEvents').text(stats.totalEvents);
    }
    if (stats.totalGuests !== undefined) {
        $('#totalGuests').text(stats.totalGuests);
    }
    if (stats.checkedInToday !== undefined) {
        $('#checkedInToday').text(stats.checkedInToday);
    }
    if (stats.totalRevenue !== undefined) {
        $('#totalRevenue').text(formatCurrency(stats.totalRevenue));
    }
}

// Update Check-in Count
function updateCheckInCount(count) {
    $('#checkInCount').text(count);
}

// Show Guest Check-in Notification
function showGuestCheckedIn(guestData) {
    const notification = `
        <div class="notification success">
            <div class="d-flex align-items-center">
                <img src="${guestData.avatar || '/images/default-avatar.png'}" 
                     class="guest-avatar-small mr-3" alt="Avatar">
                <div>
                    <strong>${guestData.fullName}</strong> đã check-in
                    <br><small>${new Date().toLocaleTimeString()}</small>
                </div>
            </div>
        </div>
    `;
    
    $('body').append(notification);
    setTimeout(() => {
        $('.notification').fadeOut();
    }, 3000);
}

// Show Guest Check-out Notification
function showGuestCheckedOut(guestData) {
    const notification = `
        <div class="notification warning">
            <div class="d-flex align-items-center">
                <img src="${guestData.avatar || '/images/default-avatar.png'}" 
                     class="guest-avatar-small mr-3" alt="Avatar">
                <div>
                    <strong>${guestData.fullName}</strong> đã check-out
                    <br><small>${new Date().toLocaleTimeString()}</small>
                </div>
            </div>
        </div>
    `;
    
    $('body').append(notification);
    setTimeout(() => {
        $('.notification').fadeOut();
    }, 3000);
}

// Format Currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// QR Code Scanner
class QRScanner {
    constructor(containerId) {
        this.container = document.getElementById(containerId);
        this.isScanning = false;
        this.html5QrcodeScanner = null;
    }

    async start() {
        if (this.isScanning) return;

        try {
            const Html5QrcodeScanner = await import('https://unpkg.com/html5-qrcode@2.3.8/html5-qrcode.min.js');
            
            this.html5QrcodeScanner = new Html5QrcodeScanner(
                this.container.id,
                { 
                    fps: 10, 
                    qrbox: { width: 250, height: 250 },
                    aspectRatio: 1.0
                }
            );

            this.html5QrcodeScanner.render((decodedText, decodedResult) => {
                this.onQRCodeScanned(decodedText);
            }, (errorMessage) => {
                // Handle scan error
            });

            this.isScanning = true;
        } catch (error) {
            console.error('QR Scanner Error:', error);
            this.showError('Không thể khởi động camera');
        }
    }

    stop() {
        if (this.html5QrcodeScanner) {
            this.html5QrcodeScanner.clear();
            this.isScanning = false;
        }
    }

    onQRCodeScanned(qrData) {
        // Send to server for processing
        this.processQRCode(qrData);
    }

    async processQRCode(qrData) {
        try {
            const response = await fetch('/api/checkin/process-qr', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ qrData: qrData })
            });

            const result = await response.json();
            
            if (result.success) {
                this.showGuestInfo(result.guest);
                this.stop();
            } else {
                this.showError(result.message);
            }
        } catch (error) {
            console.error('QR Processing Error:', error);
            this.showError('Lỗi xử lý QR code');
        }
    }

    showGuestInfo(guest) {
        const guestInfo = `
            <div class="guest-info">
                <img src="${guest.avatar || '/images/default-avatar.png'}" 
                     class="guest-avatar" alt="Avatar">
                <h4>${guest.fullName}</h4>
                <p><strong>Email:</strong> ${guest.email}</p>
                <p><strong>Phone:</strong> ${guest.phone}</p>
                <p><strong>Group:</strong> ${guest.group}</p>
                <div class="mt-3">
                    <button class="btn btn-success btn-lg" onclick="checkInGuest('${guest.id}')">
                        <i class="fas fa-check"></i> Check-in
                    </button>
                    <button class="btn btn-warning btn-lg ml-2" onclick="checkOutGuest('${guest.id}')">
                        <i class="fas fa-sign-out-alt"></i> Check-out
                    </button>
                </div>
            </div>
        `;
        
        this.container.innerHTML = guestInfo;
    }

    showError(message) {
        const errorDiv = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle"></i> ${message}
            </div>
        `;
        
        this.container.innerHTML = errorDiv;
    }
}

// Check-in Functions
async function checkInGuest(guestId) {
    try {
        const response = await fetch('/api/checkin/check-in', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ guestId: guestId })
        });

        const result = await response.json();
        
        if (result.success) {
            showNotification('Check-in thành công!', 'success');
            setTimeout(() => {
                location.reload();
            }, 2000);
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Check-in Error:', error);
        showNotification('Lỗi check-in', 'error');
    }
}

async function checkOutGuest(guestId) {
    try {
        const response = await fetch('/api/checkin/check-out', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ guestId: guestId })
        });

        const result = await response.json();
        
        if (result.success) {
            showNotification('Check-out thành công!', 'success');
            setTimeout(() => {
                location.reload();
            }, 2000);
        } else {
            showNotification(result.message, 'error');
        }
    } catch (error) {
        console.error('Check-out Error:', error);
        showNotification('Lỗi check-out', 'error');
    }
}

// Notification System
function showNotification(message, type = 'info') {
    const notification = `
        <div class="notification ${type}">
            <div class="d-flex align-items-center">
                <i class="fas fa-${getNotificationIcon(type)} mr-2"></i>
                <span>${message}</span>
            </div>
        </div>
    `;
    
    $('body').append(notification);
    setTimeout(() => {
        $('.notification').fadeOut();
    }, 3000);
}

function getNotificationIcon(type) {
    switch (type) {
        case 'success': return 'check-circle';
        case 'error': return 'exclamation-triangle';
        case 'warning': return 'exclamation-circle';
        default: return 'info-circle';
    }
}

// Data Table Functions
function initializeDataTable(tableId, options = {}) {
    const defaultOptions = {
        language: {
            url: '//cdn.datatables.net/plug-ins/1.10.24/i18n/Vietnamese.json'
        },
        responsive: true,
        pageLength: 25,
        order: [[0, 'desc']]
    };
    
    return $(tableId).DataTable({ ...defaultOptions, ...options });
}

// Chart Functions
function createLineChart(canvasId, data, options = {}) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    const defaultOptions = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            y: {
                beginAtZero: true
            }
        }
    };
    
    return new Chart(ctx, {
        type: 'line',
        data: data,
        options: { ...defaultOptions, ...options }
    });
}

function createBarChart(canvasId, data, options = {}) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    const defaultOptions = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            y: {
                beginAtZero: true
            }
        }
    };
    
    return new Chart(ctx, {
        type: 'bar',
        data: data,
        options: { ...defaultOptions, ...options }
    });
}

function createPieChart(canvasId, data, options = {}) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    const defaultOptions = {
        responsive: true,
        maintainAspectRatio: false
    };
    
    return new Chart(ctx, {
        type: 'pie',
        data: data,
        options: { ...defaultOptions, ...options }
    });
}

// Form Validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
        } else {
            input.classList.remove('is-invalid');
        }
    });

    return isValid;
}

// File Upload Preview
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            document.getElementById(previewId).src = e.target.result;
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Export Functions
function exportToExcel(tableId, filename = 'export') {
    const table = document.getElementById(tableId);
    const wb = XLSX.utils.table_to_book(table, { sheet: "Sheet1" });
    XLSX.writeFile(wb, `${filename}.xlsx`);
}

function exportToPDF(elementId, filename = 'export') {
    const element = document.getElementById(elementId);
    html2pdf().from(element).save(`${filename}.pdf`);
}

// Initialize when document is ready
$(document).ready(function() {
    // Initialize SignalR if on dashboard or check-in page
    if (window.location.pathname.includes('/Dashboard') || 
        window.location.pathname.includes('/CheckIn')) {
        initializeSignalR();
    }

    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Initialize popovers
    $('[data-toggle="popover"]').popover();

    // Auto-hide alerts
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);

    // Confirm delete actions
    $('.btn-delete').click(function(e) {
        if (!confirm('Bạn có chắc chắn muốn xóa?')) {
            e.preventDefault();
        }
    });
});
