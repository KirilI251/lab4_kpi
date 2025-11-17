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
    }
}