
export class Notify {
    constructor(title, message, type) {
        this.title = title;
        this.message = message;
        this.type = type;
    }
    
    show() {
        if (this.type === 'success') {
            showNotification(this.message, 'success');
        } else if (this.type === 'error') {
            showNotification(this.message, 'error');
        } else if (this.type === 'warning') {
            showNotification(this.message, 'warning');
        } else if (this.type === 'info') {
            showNotification(this.message, 'info');
        }
    }
    
    hide() {
        hideNotification();
    }
}