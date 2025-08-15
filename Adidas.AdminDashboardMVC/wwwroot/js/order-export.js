// Order Export Functionality
function exportReport(format) {
    console.log('Exporting report...');
    const startDate = document.getElementById('startDate')?.value;
    const endDate = document.getElementById('endDate')?.value;
    
    // Show loading indicator
    const exportButton = event?.target?.closest('.btn-group')?.querySelector('button') || 
                        document.querySelector('button[onclick^="exportReport"]');
    const originalText = exportButton.innerHTML;
    exportButton.disabled = true;
    exportButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Exporting...';
    
    // Create form data
    const formData = new FormData();
    if (startDate) formData.append('startDate', startDate);
    if (endDate) formData.append('endDate', endDate);
    
    // Get anti-forgery token
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    
    // Send request to server
    fetch(`/operation/orders/export?format=${format}`, {
        method: 'POST',
        body: formData,
        headers: token ? {
            'RequestVerificationToken': token
        } : {}
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => { 
                throw new Error(text || 'Export failed'); 
            });
        }
        return response.blob();
    })
    .then(blob => {
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = `OrderSummary_${new Date().toISOString().replace(/[:.]/g, '-')}.${format}`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        
        // Show success message
        showAlert('Export completed successfully!', 'success');
    })
    .catch(error => {
        console.error('Export error:', error);
        showAlert('Failed to generate export. Please try again.', 'danger');
    })
    .finally(() => {
        // Reset button state
        if (exportButton) {
            exportButton.disabled = false;
            exportButton.innerHTML = originalText;
        }
    });
}

function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.role = 'alert';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    
    const container = document.querySelector('.container-fluid') || document.body;
    container.insertBefore(alertDiv, container.firstChild);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
            const bsAlert = new bootstrap.Alert(alertDiv);
            bsAlert.close();
        } else {
            alertDiv.remove();
        }
    }, 5000);
}

// Clear date inputs
function clearDates() {
    const startDate = document.getElementById('startDate');
    const endDate = document.getElementById('endDate');
    
    if (startDate) startDate.value = '';
    if (endDate) endDate.value = '';
}
