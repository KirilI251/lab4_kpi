using Energy_Project.Services.Interfaces;
using Moq;
using Energy_Project.Models;
using Energy_Project.Services;

namespace SmartHomeTests
{
    public class EnergyMonitorServiceTests
    {
        private readonly Mock<IDeviceRepository> _deviceRepo;
        private readonly Mock<IEnergyPlanRepository> _planRepo;
        private readonly Mock<INotificationService> _notify;

        private readonly EnergyMonitorService _service; // Наш сервіс (System Under Test)

        // Конструктор, який виконується перед кожним тестом
        public EnergyMonitorServiceTests()
        {
            // Ініціалізуємо всі моки тут (найкраща практика для xUnit)
            _deviceRepo = new Mock<IDeviceRepository>();
            _planRepo = new Mock<IEnergyPlanRepository>();
            _notify = new Mock<INotificationService>();

            // Ініціалізуємо сервіс, передаючи йому *об'єкти* наших моків
            _service = new EnergyMonitorService(
                _deviceRepo.Object,
                _planRepo.Object,
                _notify.Object
            );
        }

        /// <summary>
        /// Тест перевіряє, що CalculateCurrentUsageKwh коректно розраховує
        /// сумарне споживання енергії (в кВт/год) *лише* увімкнених пристроїв.
        /// </summary>
        [Fact]
        public void CalculateCurrentUsageKwh_ShouldReturnCorrectSum_WhenDevicesAreOn()
        {
            // Arrange
            var devices = new List<Device>
            {
                new Device { Id = 1, Name = "Heater", IsOn = true, PowerUsageWatts = 1000 },
                new Device { Id = 2, Name = "TV", IsOn = true, PowerUsageWatts = 500 },
                new Device { Id = 3, Name = "PC", IsOn = false, PowerUsageWatts = 800 } // Цей пристрій вимкнено
            };

            // Налаштовуємо _deviceRepo, щоб він повернув наш список пристроїв
            _deviceRepo.Setup(repo => repo.GetAll()).Returns(devices);

            // Очікуваний результат: (1000W + 500W) / 1000.0 = 1.5 kWh
            double expectedKwh = 1.5;

            // Act
            // Викликаємо метод, логіку якого ми тестуємо
            double actualKwh = _service.CalculateCurrentUsageKwh();

            // Assert
            // (Тип перевірки: Assert.Equal)
            Assert.Equal(expectedKwh, actualKwh);
        }

        /// <summary>
        /// Тест перевіряє, що розрахунок споживання повертає 0,
        /// якщо всі пристрої вимкнені, незалежно від їх потужності.
        /// </summary>
        [Theory] // (Тип перевірки: [Theory])
        [InlineData(1000, 500)] // (Тип перевірки: [InlineData]) - Сценарій 1
        [InlineData(2000, 3000)] // Сценарій 2
        [InlineData(0, 0)] // Сценарій 3
        public void CalculateCurrentUsageKwh_ShouldReturnZero_WhenAllDevicesAreOff(double power1, double power2)
        {
            // Arrange
            var devices = new List<Device>
            {
                new Device { IsOn = false, PowerUsageWatts = power1 },
                new Device { IsOn = false, PowerUsageWatts = power2 }
            };

            _deviceRepo.Setup(repo => repo.GetAll()).Returns(devices);

            double expectedKwh = 0.0;

            // Act
            double actualKwh = _service.CalculateCurrentUsageKwh();

            // Assert
            Assert.Equal(expectedKwh, actualKwh);
        }

        /// <summary>
        /// Тест перевіряє, що метод CheckForOverload відправляє сповіщення,
        /// коли споживання енергії перевищує денний ліміт.
        /// </summary>
        [Fact]
        public void CheckForOverload_ShouldSendAlert_WhenUsageExceedsLimit()
        {
            // Arrange
            // 1. Налаштовуємо споживання (usage):
            // Робимо так, щоб CalculateCurrentUsageKwh() повертав 1.5
            var devices = new List<Device> { new Device { IsOn = true, PowerUsageWatts = 1500 } };
            _deviceRepo.Setup(repo => repo.GetAll()).Returns(devices);

            // 2. Налаштовуємо ліміт (plan):
            // Встановлюємо ліміт 1.0 (менше, ніж споживання)
            var plan = new EnergyPlan { DailyLimitKwh = 1.0 };
            _planRepo.Setup(repo => repo.GetCurrentPlan()).Returns(plan);

            // Act
            _service.CheckForOverload();

            // Assert
            // Перевіряємо, що _notify.SendAlert() був викликаний 1 раз
            // (Тип перевірки: Verify(..., Times.AtLeastOnce) або Times.Once)
            // Це наш 10-й унікальний тип перевірки.
            _notify.Verify(n => n.SendAlert(It.IsAny<string>()), Times.Once());
        }

        /// <summary>
        /// Тест перевіряє, що метод CheckForOverload НЕ відправляє сповіщення,
        /// коли споживання енергії знаходиться в межах ліміту.
        /// </summary>
        [Fact]
        public void CheckForOverload_ShouldNotSendAlert_WhenUsageIsWithinLimit()
        {
            // Arrange
            // 1. Налаштовуємо споживання (usage):
            // Споживання 0.5 kWh
            var devices = new List<Device> { new Device { IsOn = true, PowerUsageWatts = 500 } };
            _deviceRepo.Setup(repo => repo.GetAll()).Returns(devices);

            // 2. Налаштовуємо ліміт (plan):
            // Ліміт 1.0 kWh (більше, ніж споживання)
            var plan = new EnergyPlan { DailyLimitKwh = 1.0 };
            _planRepo.Setup(repo => repo.GetCurrentPlan()).Returns(plan);

            // Act
            _service.CheckForOverload();

            // Assert
            // Перевіряємо, що _notify.SendAlert() НЕ був викликаний
            // (Тип перевірки: Verify(..., Times.Never) - наш 11-й унікальний тип)
            _notify.Verify(n => n.SendAlert(It.IsAny<string>()), Times.Never());
        }
    }
}