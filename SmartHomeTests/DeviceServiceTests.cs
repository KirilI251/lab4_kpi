using Energy_Project.Services.Interfaces;
using Moq;
using Energy_Project.Models;
using Energy_Project.Services;

namespace SmartHomeTests
{
    public class DeviceServiceTests
    {
        private readonly Mock<IDeviceRepository> _deviceRepo;
        private readonly DeviceService _service; // Наш сервіс, який ми тестуємо

        // Конструктор, який виконується перед кожним тестом
        public DeviceServiceTests()
        {
            // Створюємо нові моки для кожної тестової ситуації
            _deviceRepo = new Mock<IDeviceRepository>();
            
            // Ініціалізуємо сервіс, "вставляючи" в нього наш мок-репозиторій
            _service = new DeviceService(_deviceRepo.Object);
        }

        /// <summary>
        /// Тест перевіряє, що метод ToggleDevice коректно вмикає пристрій (повертає true)
        /// і викликає метод Update у репозиторії.
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldTurnDeviceOn_WhenCalledWithTrue()
        {
            // Arrange (Підготовка)
            var device = new Device { Id = 1, Name = "Lamp", IsOn = false };
            int deviceId = 1;

            // Налаштовуємо мок-репозиторій:
            // Коли буде викликаний GetById(1), він має повернути наш 'device'
            _deviceRepo.Setup(repo => repo.GetById(deviceId)).Returns(device);

            // Act (Дія)
            // Викликаємо метод, який тестуємо
            bool result = _service.ToggleDevice(deviceId, true);

            // Assert (Перевірка)
            // 1. Перевіряємо, що метод повернув 'true' (Тип перевірки: Assert.True)
            Assert.True(result);

            // 2. Перевіряємо, що метод Update() нашого репозиторію
            //    був викликаний рівно 1 раз. (Тип перевірки: Verify(..., Times.Exactly(1)))
            _deviceRepo.Verify(repo => repo.Update(device), Times.Exactly(1));
        }

        /// <summary>
        /// Тест перевіряє, що метод ToggleDevice коректно вимикає пристрій (повертає false).
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldTurnDeviceOff_WhenCalledWithFalse()
        {
            // Arrange
            // Цього разу пристрій спочатку увімкнений
            var device = new Device { Id = 2, Name = "TV", IsOn = true };
            int deviceId = 2;

            _deviceRepo.Setup(repo => repo.GetById(deviceId)).Returns(device);

            // Act
            bool result = _service.ToggleDevice(deviceId, false);

            // Assert
            // 1. Перевіряємо, що метод повернув 'false'
            // (Тип перевірки: Assert.False - наш 3-й унікальний тип)
            Assert.False(result);

            // 2. Також перевіряємо, що Update було викликано
            _deviceRepo.Verify(repo => repo.Update(device), Times.Once());
        }

        /// <summary>
        /// Тест перевіряє, що метод ToggleDevice кидає виняток ArgumentException,
        /// якщо пристрій із заданим id не знайдено.
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldThrowArgumentException_WhenDeviceNotFound()
        {
            // Arrange
            int nonExistentDeviceId = 99;

            // Налаштовуємо мок-репозиторій:
            // Коли буде викликаний GetById(99), він має повернути null
            _deviceRepo.Setup(repo => repo.GetById(nonExistentDeviceId))
                       .Returns((Device?)null); // Явно вказуємо, що повертаємо null

            // Act & Assert
            // Ми перевіряємо, що виклик методу _service.ToggleDevice
            // призведе до винятку типу ArgumentException
            // (Тип перевірки: Assert.Throws - наш 4-й унікальний тип)
            Assert.Throws<ArgumentException>(() => _service.ToggleDevice(nonExistentDeviceId, true));

            // Додаткова перевірка: переконатися, що Update НЕ викликався
            _deviceRepo.Verify(repo => repo.Update(It.IsAny<Device>()), Times.Never());
        }
    }
}