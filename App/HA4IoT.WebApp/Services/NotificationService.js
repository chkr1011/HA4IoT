function createNotificationService(apiService) {
    var srv = this;

    srv.delete = function (notificationId) {
        apiService.executeApi("Service/INotificationService/Delete", { "Uid": notificationId });
    };

    return this;
}