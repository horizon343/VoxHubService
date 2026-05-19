<div align="center">

  <h1>🧊 VoxHubService</h1>

  <p>
    <b>Backend-сервис для хранения, восстановления и версионирования voxel-моделей в формате <code>.vox</code>.</b>
  </p>

  <p>
    VoxHubService принимает voxel-модели от desktop-клиента VoxHub, разбивает их на chunks,
    сохраняет данные в S3-совместимое хранилище, ведёт историю snapshot и commit-версий,
    а также позволяет восстановить нужную версию обратно в <code>.vox</code>.
  </p>

  <p>
    <img src="https://img.shields.io/badge/C%23-100%25-512BD4?style=for-the-badge&logo=csharp&logoColor=white" alt="C#">
    <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 10">
    <img src="https://img.shields.io/badge/gRPC-Backend-2CA5E0?style=for-the-badge" alt="gRPC">
    <img src="https://img.shields.io/badge/PostgreSQL-Database-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL">
    <img src="https://img.shields.io/badge/S3-Object_Storage-FF9900?style=for-the-badge&logo=amazons3&logoColor=white" alt="S3">
    <img src="https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white" alt="Docker">
  </p>

  <p>
    <b>📦 Snapshots</b> · <b>🌿 Commits</b> · <b>🧱 Chunks</b> · <b>☁️ S3</b> · <b>🔌 gRPC</b>
  </p>

</div>

<hr>

<h2>✨ Возможности</h2>

<table>
  <tr>
    <td>📤 <b>Snapshot import</b></td>
    <td>Загрузка начальной версии voxel-модели из файла <code>.vox</code>.</td>
  </tr>
  <tr>
    <td>🌿 <b>Commit import</b></td>
    <td>Создание новой версии модели поверх уже существующей истории.</td>
  </tr>
  <tr>
    <td>🕓 <b>История версий</b></td>
    <td>Хранение и выдача списка версий для выбранной модели.</td>
  </tr>
  <tr>
    <td>📚 <b>Каталог моделей</b></td>
    <td>Получение списка доступных voxel-моделей для клиента VoxHub.</td>
  </tr>
  <tr>
    <td>🧱 <b>Chunk-based storage</b></td>
    <td>Разбиение voxel-моделей на chunks для хранения и переиспользования данных.</td>
  </tr>
  <tr>
    <td>☁️ <b>S3-хранилище</b></td>
    <td>Сохранение бинарных chunk-данных в S3-совместимый object storage.</td>
  </tr>
  <tr>
    <td>🐘 <b>PostgreSQL</b></td>
    <td>Хранение метаданных моделей, версий и связей между chunks.</td>
  </tr>
  <tr>
    <td>📥 <b>Restore</b></td>
    <td>Восстановление выбранной версии модели обратно в файл <code>.vox</code>.</td>
  </tr>
  <tr>
    <td>🔌 <b>gRPC API</b></td>
    <td>Обмен данными с desktop-клиентом VoxHub через gRPC-сервисы.</td>
  </tr>
  <tr>
    <td>🐳 <b>Docker</b></td>
    <td>Запуск backend-сервиса в контейнере с передачей настроек через <code>.env</code>.</td>
  </tr>
</table>

<hr>

<h2>🖼️ Что делает VoxHubService</h2>

<p>
  <b>VoxHubService</b> — это backend-часть проекта VoxHub. Сервис отвечает за приём
  voxel-моделей, их нормализацию, разбиение на chunks, сохранение бинарных данных
  в object storage и хранение метаданных в PostgreSQL.
</p>

<p>
  Клиент VoxHub не работает напрямую с базой данных или S3. Вместо этого он обращается
  к backend через gRPC: загружает snapshot, отправляет commit, запрашивает список моделей,
  получает историю версий и скачивает восстановленную <code>.vox</code>-модель.
</p>

<p>
  Такой подход позволяет хранить историю изменений voxel-модели не как набор случайных файлов,
  а как управляемую последовательность версий, которую можно просматривать, скачивать и сравнивать
  на стороне desktop-клиента.
</p>

<hr>

<h2>🏗️ Архитектура проекта</h2>

<pre><code>VoxHubService/
├── Application/        # Pipeline-логика snapshot, commit и restore
│
├── DB/                 # DbContext и сущности для PostgreSQL
│
├── Domain/             # Доменная логика voxel-моделей
│   ├── Canonical/      # Каноническое представление voxel-данных
│   ├── Chunking/       # Разбиение модели на chunks
│   ├── Exporting/      # Экспорт модели обратно в .vox
│   ├── Importing/      # Импорт .vox-файлов
│   ├── Serialization/  # Сериализация chunk-данных для хранения
│   └── Vox/            # Работа с VOX-структурами
│
├── Interfaces/         # Общие интерфейсы сервисов
│
├── Protos/             # gRPC-контракты
│   ├── CommitImportService.proto
│   ├── ModelQueryService.proto
│   ├── ModelRestoreService.proto
│   ├── SnapshotImportService.proto
│   └── VersionQueryService.proto
│
├── Services/           # Реализации gRPC-сервисов
│
├── Storage/            # S3 object storage adapter
│
├── Program.cs          # Конфигурация приложения, DI, DB, S3 и gRPC
└── VoxHubService.csproj</code></pre>

<hr>

<h2>⚙️ Технологии</h2>

<table>
  <thead>
    <tr>
      <th>Технология</th>
      <th>Для чего используется</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>💜 <b>C#</b></td>
      <td>Основной язык backend-сервиса</td>
    </tr>
    <tr>
      <td>🟣 <b>.NET 10.0</b></td>
      <td>Целевая платформа backend-приложения</td>
    </tr>
    <tr>
      <td>🔌 <b>gRPC</b></td>
      <td>API для взаимодействия с desktop-клиентом VoxHub</td>
    </tr>
    <tr>
      <td>🐘 <b>PostgreSQL</b></td>
      <td>Хранение моделей, версий и метаданных chunks</td>
    </tr>
    <tr>
      <td>🧩 <b>Entity Framework Core</b></td>
      <td>Работа с базой данных через DbContext и сущности</td>
    </tr>
    <tr>
      <td>☁️ <b>S3-compatible storage</b></td>
      <td>Хранение бинарных chunk-объектов</td>
    </tr>
    <tr>
      <td>📨 <b>Google.Protobuf</b></td>
      <td>Генерация и обработка protobuf-сообщений</td>
    </tr>
    <tr>
      <td>🧊 <b>.vox</b></td>
      <td>Формат импортируемых и экспортируемых voxel-моделей</td>
    </tr>
    <tr>
      <td>🐳 <b>Docker</b></td>
      <td>Контейнеризация backend-сервиса</td>
    </tr>
  </tbody>
</table>

<hr>

<h2>🔌 gRPC-сервисы</h2>

<p>
  Backend предоставляет несколько gRPC-сервисов для работы с моделями и версиями:
</p>

<table>
  <thead>
    <tr>
      <th>Сервис</th>
      <th>Назначение</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>📤 <b>SnapshotImportService</b></td>
      <td>Загрузка первой версии voxel-модели</td>
    </tr>
    <tr>
      <td>🌿 <b>CommitImportService</b></td>
      <td>Создание новой версии существующей модели</td>
    </tr>
    <tr>
      <td>📥 <b>ModelRestoreService</b></td>
      <td>Восстановление выбранной версии в <code>.vox</code></td>
    </tr>
    <tr>
      <td>📚 <b>ModelQueryService</b></td>
      <td>Получение списка доступных моделей</td>
    </tr>
    <tr>
      <td>🕓 <b>VersionQueryService</b></td>
      <td>Получение истории версий выбранной модели</td>
    </tr>
  </tbody>
</table>

<hr>

<h2>🔐 Переменные окружения</h2>

<p>
  Для запуска backend нужны настройки PostgreSQL и S3-compatible хранилища.
  Создайте файл <code>.env</code> в корне проекта или передайте переменные окружения
  через Docker / hosting-платформу.
</p>

<pre><code>VOXHUB_DB="Host=your-postgres-host;Port=5432;Database=your-db;Username=your-user;Password=your-password"

VOXHUB_S3_ACCESS_KEY="your-s3-access-key"
VOXHUB_S3_SECRET_KEY="your-s3-secret-key"
VOXHUB_S3_ENDPOINT="https://your-s3-endpoint"
VOXHUB_S3_REGION="your-s3-region"
VOXHUB_S3_BUCKET="your-s3-bucket"</code></pre>

<hr>

<h2>🚀 Быстрый старт без Docker</h2>

<p>
  Для локального запуска нужен установленный .NET SDK с поддержкой <code>net10.0</code>,
  доступная PostgreSQL-база и S3-compatible хранилище.
</p>

<pre><code>git clone https://github.com/horizon343/VoxHubService.git
cd VoxHubService

dotnet restore
dotnet build
dotnet run --project VoxHubService/VoxHubService.csproj</code></pre>

<p>
  По умолчанию адрес запуска зависит от настроек ASP.NET Core.
  Для совместимости с desktop-клиентом VoxHub удобно запускать backend на:
</p>

<pre><code>http://localhost:5152</code></pre>

<hr>

<h2>🐳 Запуск через Docker</h2>

<p>
  В корне репозитория должен находиться <code>Dockerfile</code>.
  Также рядом можно создать файл <code>.env</code> с переменными окружения.
</p>

<h3>1. Сборка образа</h3>

<pre><code>docker build -t voxhub-service .</code></pre>

<h3>2. Запуск контейнера</h3>

<pre><code>docker run --env-file .env -p 5152:8080 voxhub-service</code></pre>

<p>
  Внутри контейнера сервис слушает порт <code>8080</code>, а на хосте он будет доступен
  по адресу:
</p>

<pre><code>http://localhost:5152</code></pre>

<hr>

<h2>📋 Требования</h2>

<ul>
  <li>🟣 .NET SDK с поддержкой <code>net10.0</code> — для локального запуска без Docker</li>
  <li>🐳 Docker — для контейнерного запуска</li>
  <li>🐘 PostgreSQL — для хранения метаданных</li>
  <li>☁️ S3-compatible object storage — для хранения chunk-данных</li>
  <li>🧊 <code>.vox</code>-файлы — для загрузки snapshot и commit-версий</li>
  <li>🖥️ VoxHub desktop client — для удобной работы через GUI</li>
</ul>

<hr>

<h2>🧠 Как хранятся модели</h2>

<p>
  VoxHubService не обязан хранить каждую версию модели как отдельный полный файл.
  Вместо этого модель импортируется в каноническое voxel-представление, после чего данные
  разбиваются на chunks.
</p>

<p>
  Chunk-данные сохраняются в S3-compatible хранилище, а связи между моделями, версиями
  и chunks сохраняются в PostgreSQL. Благодаря этому backend может восстановить нужную
  версию модели и отдать её клиенту обратно в формате <code>.vox</code>.
</p>

<hr>

<h2>🛠️ Статус проекта</h2>

<p>
  VoxHubService находится в активной разработке. Основной фокус проекта —
  стабильное хранение версий voxel-моделей, восстановление <code>.vox</code>-файлов
  и интеграция с desktop-клиентом VoxHub.
</p>

<p>
  Возможные направления развития:
</p>

<ul>
  <li>📦 компактная сериализация chunk-данных;</li>
  <li>🔎 улучшение поиска и фильтрации моделей;</li>
  <li>🧪 автоматические тесты import / commit / restore pipeline;</li>
  <li>📊 метрики размера моделей и chunks;</li>
  <li>🛡️ более строгая валидация входящих <code>.vox</code>-файлов;</li>
  <li>🚀 публикация готового Docker image.</li>
</ul>

<hr>

<h2>🤝 Contributing</h2>

<p>
  Идеи, улучшения и pull requests приветствуются. Если вы хотите улучшить хранение chunks,
  оптимизировать импорт <code>.vox</code>, доработать gRPC API или улучшить интеграцию
  с S3/PostgreSQL — создайте issue или pull request.
</p>

<hr>

<div align="center">

  <p>
    <b>🧊 VoxHubService</b> — backend для истории, хранения и восстановления voxel-моделей.
  </p>

  <p>
    <sub>Built with 💜 C#, gRPC, PostgreSQL, S3 and voxel magic.</sub>
  </p>

</div>