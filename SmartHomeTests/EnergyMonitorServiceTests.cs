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
    }
}