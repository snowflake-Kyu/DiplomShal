using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFPPShall
{
    public class TeacherDataIntegrity
    {
        // ========== ЗАЩИЩЁННЫЕ ПОЛЯ (инкапсуляция) ==========
        private string _originalHash;           // хранит хэш исходных данных
        private DateTime _lastCheckTime;        // время последней проверки
        private int _totalRecords;              // количество записей
        private List<TeacherRecord> _records;   // копия данных учителей

        // Вложенный класс для хранения записи об учителе
        public class TeacherRecord
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
        }

        // ========== КОНСТРУКТОРЫ ==========

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public TeacherDataIntegrity()
        {
            _originalHash = string.Empty;
            _lastCheckTime = DateTime.Now;
            _totalRecords = 0;
            _records = new List<TeacherRecord>();
        }

        /// <summary>
        /// Конструктор с инициализацией списка учителей
        /// </summary>
        /// <param name="teachers">Список учителей из DataTable</param>
        public TeacherDataIntegrity(DataTable teachersTable)
        {
            _records = ConvertDataTableToRecordList(teachersTable);
            _totalRecords = _records.Count;
            _originalHash = ComputeHash(_records);
            _lastCheckTime = DateTime.Now;
        }

        // ========== СВОЙСТВА (get/set с логикой) ==========

        /// <summary>
        /// Хэш данных (только для чтения)
        /// </summary>
        public string DataHash
        {
            get { return _originalHash; }
            private set { _originalHash = value; }
        }

        /// <summary>
        /// Время последней проверки целостности
        /// </summary>
        public DateTime LastCheckTime
        {
            get { return _lastCheckTime; }
            private set { _lastCheckTime = value; }
        }

        /// <summary>
        /// Количество записей
        /// </summary>
        public int TotalRecords
        {
            get { return _totalRecords; }
            private set { _totalRecords = value; }
        }

        /// <summary>
        /// Статус целостности данных (только для чтения)
        /// </summary>
        public bool IsIntegrityValid
        {
            get { return VerifyIntegrity(); }
        }

        /// <summary>
        /// Текстовое описание статуса
        /// </summary>
        public string StatusMessage
        {
            get
            {
                if (VerifyIntegrity())
                    return "✓ Целостность подтверждена";
                else
                    return "⚠ ДАННЫЕ БЫЛИ ИЗМЕНЕНЫ ИЗВНЕ!";
            }
        }

        // ========== МЕТОДЫ ==========

        /// <summary>
        /// Преобразует DataTable в список записей (вспомогательный метод)
        /// </summary>
        private List<TeacherRecord> ConvertDataTableToRecordList(DataTable table)
        {
            var list = new List<TeacherRecord>();
            if (table == null) return list;

            foreach (DataRow row in table.Rows)
            {
                var record = new TeacherRecord
                {
                    Id = Convert.ToInt32(row["TeacherID"]),
                    FullName = row["FullName"]?.ToString() ?? "",
                    Email = row["Email"]?.ToString() ?? ""
                };
                list.Add(record);
            }
            return list;
        }

        /// <summary>
        /// Хэш-функция SHA256 для списка учителей
        /// Именно здесь используется криптографическая хэш-функция
        /// </summary>
        private string ComputeHash(List<TeacherRecord> records)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // Формируем строковое представление всех записей
                StringBuilder dataString = new StringBuilder();
                foreach (var record in records.OrderBy(r => r.Id))
                {
                    dataString.AppendLine($"{record.Id}|{record.FullName}|{record.Email}");
                }

                // Вычисляем хэш
                byte[] inputBytes = Encoding.UTF8.GetBytes(dataString.ToString());
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Возвращаем хэш в Base64 (компактное представление)
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Обновляет хэш после сохранения изменений
        /// </summary>
        public void UpdateHash(DataTable newTeachersTable)
        {
            _records = ConvertDataTableToRecordList(newTeachersTable);
            _totalRecords = _records.Count;
            _originalHash = ComputeHash(_records);
            _lastCheckTime = DateTime.Now;
        }

        /// <summary>
        /// Проверяет, не изменились ли данные
        /// </summary>
        private bool VerifyIntegrity()
        {
            if (_records == null || _records.Count == 0)
                return true;

            string currentHash = ComputeHash(_records);
            return currentHash == _originalHash;
        }

        /// <summary>
        /// Сравнивает текущие данные с переданными
        /// </summary>
        public bool CompareWithCurrentData(DataTable currentTable)
        {
            var currentRecords = ConvertDataTableToRecordList(currentTable);
            string currentHash = ComputeHash(currentRecords);
            return currentHash == _originalHash;
        }

        /// <summary>
        /// Возвращает детальную информацию о состоянии (для отображения)
        /// </summary>
        public string GetDetailedStatus()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"📊 Статус целостности данных учителей");
            sb.AppendLine($"=====================================");
            sb.AppendLine($"📅 Последняя проверка: {_lastCheckTime:dd.MM.yyyy HH:mm:ss}");
            sb.AppendLine($"📋 Всего записей: {_totalRecords}");
            sb.AppendLine($"🔒 Хэш (SHA256): {(_originalHash?.Length > 30 ? _originalHash.Substring(0, 30) + "..." : _originalHash ?? "нет")}");
            sb.AppendLine($"✅ Целостность: {(VerifyIntegrity() ? "ПОДТВЕРЖДЕНА" : "НАРУШЕНА")}");
            return sb.ToString();
        }
    }
}
