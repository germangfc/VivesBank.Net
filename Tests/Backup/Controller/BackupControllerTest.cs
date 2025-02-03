using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VivesBankApi.Controllers;
using VivesBankApi.Backup.Service;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Legacy;
using VivesBankApi.Backup;
using Path = System.IO.Path;
/*
[TestFixture]
[TestOf(typeof(BackupController))]
public class BackupControllerTest
    {
    private Mock<IBackupService> _mockBackupService;
    private Mock<ILogger<BackupController>> _mockLogger;
    private BackupController _controller;

    [SetUp]
    public void Setup()
    {
        _mockBackupService = new Mock<IBackupService>();
        _mockLogger = new Mock<ILogger<BackupController>>();
        _controller = new BackupController(_mockBackupService.Object);
    }

}
*/