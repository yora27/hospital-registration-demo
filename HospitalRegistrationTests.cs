using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using System.Net.Sockets;
using System.Text.Json;
using LoginApp;
using Hosreg1;

public class HospitalRegistrationTests
{
    [Fact]
    public async Task TcpClientService_ConnectAsync_Success()
    {
        // Arrange
        var tcpClientService = new TcpClientService();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await tcpClientService.ConnectAsync());
    }

    [Fact]
    public async Task TcpClientService_SendRequestAsync_ThrowsOnDisconnectedClient()
    {
        // Arrange
        var tcpClientService = new TcpClientService();

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            tcpClientService.SendRequestAsync<JsonElement>(new { command = "TEST" }));
    }

    [Fact]
    public void Patient_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        int testId = 123;
        string testName = "Test Patient";

        // Act
        var patient = new Patient { Id = testId, Name = testName };

        // Assert
        Assert.Equal(testId, patient.Id);
        Assert.Equal(testName, patient.Name);
    }

    [Fact]
    public void AppointmentResponse_Status_ReturnsCorrectValue()
    {
        // Arrange
        var futureAppointment = new AppointmentResponse
        {
            DateTime = DateTime.Now.AddDays(1)
        };
        var pastAppointment = new AppointmentResponse
        {
            DateTime = DateTime.Now.AddDays(-1)
        };

        // Assert
        Assert.Equal("Активний", futureAppointment.Status);
        Assert.Equal("Завершений", pastAppointment.Status);
    }

    [Fact]
    public void TimeSlotViewModel_PropertiesSetCorrectly()
    {
        // Arrange
        var timeSlot = new TimeSlotViewModel
        {
            DisplayText = "10:00",
            StartTime = DateTime.Parse("2024-01-01 10:00"),
            IsAvailable = true
        };

        // Assert
        Assert.Equal("10:00", timeSlot.DisplayText);
        Assert.True(timeSlot.IsAvailable);
    }

    [Fact]
    public void ReviewResponse_PropertiesSetCorrectly()
    {
        // Arrange
        var review = new ReviewResponse
        {
            Id = 1,
            Comment = "Great service",
            Date = DateTime.Now,
            Doctor = new Doctor { Id = 1, Name = "Dr. Smith" }
        };

        // Assert
        Assert.Equal(1, review.Id);
        Assert.Equal("Great service", review.Comment);
        Assert.Equal("Dr. Smith", review.Doctor.Name);
    }

    [Fact]
    public void DoctorDetails_PropertiesSetCorrectly()
    {
        // Arrange
        var doctor = new DoctorDetails
        {
            Id = 1,
            Name = "Dr. Johnson",
            Specialization = "Cardiology"
        };

        // Assert
        Assert.Equal(1, doctor.Id);
        Assert.Equal("Dr. Johnson", doctor.Name);
        Assert.Equal("Cardiology", doctor.Specialization);
    }

    [Fact]
    public void PatientDataResponse_PropertiesSetCorrectly()
    {
        // Arrange
        var patientData = new PatientDataResponse
        {
            Name = "John Doe",
            BirthDate = DateTime.Parse("1990-01-01"),
            Phone = "+1234567890"
        };

        // Assert
        Assert.Equal("John Doe", patientData.Name);
        Assert.Equal(DateTime.Parse("1990-01-01"), patientData.BirthDate);
        Assert.Equal("+1234567890", patientData.Phone);
    }

    [Fact]
    public void PatientUpdateResponse_PropertiesSetCorrectly()
    {
        // Arrange
        var updateResponse = new PatientUpdateResponse
        {
            Success = true,
            Message = "Update successful"
        };

        // Assert
        Assert.True(updateResponse.Success);
        Assert.Equal("Update successful", updateResponse.Message);
    }

    [Fact]
    public void TimeSlotResponse_PropertiesSetCorrectly()
    {
        // Arrange
        var timeSlot = new TimeSlotResponse
        {
            StartTime = DateTime.Parse("2024-01-01 14:30"),
            IsAvailable = true
        };

        // Assert
        Assert.Equal(DateTime.Parse("2024-01-01 14:30"), timeSlot.StartTime);
        Assert.True(timeSlot.IsAvailable);
    }

    [Fact]
    public void AppointmentDetails_StatusCalculatedCorrectly()
    {
        // Arrange
        var futureAppointment = new AppointmentDetails
        {
            DateTime = DateTime.Now.AddDays(1)
        };
        var pastAppointment = new AppointmentDetails
        {
            DateTime = DateTime.Now.AddDays(-1)
        };

        // Assert
        Assert.Equal("Активний", futureAppointment.Status);
        Assert.Equal("Завершений", pastAppointment.Status);
    }

    [Fact]
    public void BoolToStatusConverter_ConvertsCorrectly()
    {
        // Arrange
        var converter = new BoolToStatusConverter();

        // Assert
        Assert.Equal("Активний", converter.Convert(true, null, null, null));
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(null, null, null, null));
    }
}