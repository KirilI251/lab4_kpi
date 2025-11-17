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

        /// <summary>
        /// Тест перевіряє, що метод GetActiveDevices повертає список, 
        /// який містить *лише* увімкнені пристрої.
        /// </summary>
        [Fact]
        public void GetActiveDevices_ShouldReturnOnlyOnDevices_WhenCalled()
        {
            // Arrange
            var deviceOn = new Device { Id = 1, Name = "Lamp", IsOn = true };
            var deviceOff = new Device { Id = 2, Name = "TV", IsOn = false };
            var allDevices = new List<Device> { deviceOn, deviceOff };

            // Налаштовуємо мок-репозиторій:
            // Коли буде викликаний GetAll(), він поверне наш повний список
            _deviceRepo.Setup(repo => repo.GetAll()).Returns(allDevices);

            // Act
            var result = _service.GetActiveDevices();

            // Assert
            // 1. Перевіряємо, що результат не 'null'
            // (Тип перевірки: Assert.NotNull - наш 7-й унікальний тип)
            Assert.NotNull(result);

            // 2. Перевіряємо, що в результаті *лише* 1 пристрій
            // (Тип перевірки: Assert.Equal - 5-й унікальний тип)
            Assert.Equal(1, result.Count());

            // 3. Перевіряємо, що в результаті є саме той, що увімкнений
            // (Тип перевірки: Assert.Contains - 6-й унікальний тип)
            Assert.Contains(deviceOn, result);
        }

        /// <summary>
        /// Тест перевіряє, що метод GetActiveDevices повертає порожній список,
        /// якщо немає увімкнених пристроїв.
        /// </summary>
        [Fact]
        public void GetActiveDevices_ShouldReturnEmptyList_WhenNoDevicesAreOn()
        {
            // Arrange
            var deviceOff1 = new Device { Id = 1, Name = "Lamp", IsOn = false };
            var deviceOff2 = new Device { Id = 2, Name = "TV", IsOn = false };
            var allDevices = new List<Device> { deviceOff1, deviceOff2 };

            _deviceRepo.Setup(repo => repo.GetAll()).Returns(allDevices);

            // Act
            var result = _service.GetActiveDevices();

            // Assert
            // 1. Перевіряємо, що результат не 'null' (хороша практика)
            Assert.NotNull(result);

            // 2. Перевіряємо, що список порожній
            // (Тип перевірки: Assert.Empty - наш 8-й унікальний тип)
            Assert.Empty(result);
        }

        /// <summary>
        /// Тест перевіряє, що стан пристрою (IsOn) змінюється
        /// на протилежний після виклику ToggleDevice.
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldChangeDeviceState_WhenToggled()
        {
            // Arrange
            var device = new Device { Id = 1, Name = "Lamp", IsOn = false };
            bool initialState = device.IsOn; // Запам'ятовуємо початковий стан (false)

            _deviceRepo.Setup(repo => repo.GetById(device.Id)).Returns(device);

            // Act
            // Викликаємо метод, щоб увімкнути пристрій
            _service.ToggleDevice(device.Id, true); 

            // Assert
            // Перевіряємо, що новий стан (device.IsOn) НЕ дорівнює початковому
            // (Тип перевірки: Assert.NotEqual - наш 15-й унікальний тип)
            Assert.NotEqual(initialState, device.IsOn);
        }
    }
}