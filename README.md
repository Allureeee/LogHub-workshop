# LogHub-workshop
[![build](https://github.com/Allureeee/LogHub-workshop/actions/workflows/build.yml/badge.svg)](https://github.com/Allureeee/LogHub-workshop/actions/workflows/build.yml)

**LogHub**: система сбора и обработки логов на C#, которая постепенно растёт от одного «god-класса» до архитектуры на паттернах проектирования. Учебный проект курса «Технологии и методы программирования», раздел «Паттерны программирования».

## Автор
- **Кузнецова Елизавета**
- GitHub: [@Allureeee](https://github.com/Allureeee)
- Email: `elizavetaku2006@ya.ru`
- Группа: `БИСО-01-24`

## О проекте

*Идея курса:  все пять лабораторных строят одну систему, и каждый паттерн появляется тогда, когда проект сам упирается в проблему, которую он решает.*

### Что за система 

**Источники** (файл, syslog, HTTP, БД) → **Импорт** (Стратегия, Шаблонный метод) → **Обработка** (Цепочка, Посетитель, Наблюдатель) → **Приёмники** (консоль, файл, БД, ELK — через Адаптеры)


### Вариант 20

- **Источник:** смешанный поток, два формата записей вперемешку
- **Приёмники:** консоль и файл
- **Особенность:** автоопределение формата каждой записи

## Статус

| Лабораторная | Тема | Паттерны | Статус |
|---|---|---|---|
| 1 (ЛР 4) | Git, среда, рефакторинг god-класса | SOLID | 🚧 в работе |
| 2 (ЛР 5) | IoC, DI-контейнер | Singleton, DI | ⏳ запланировано |
| 3 (ЛР 6) | Импорт логов, обход графа | Strategy, Template Method, Iterator | ⏳ запланировано |
| 4 (ЛР 7) | Конвейер обработки | Chain of Responsibility, Visitor, Observer | ⏳ запланировано |
| 5 (ЛР 8) | Сборка системы | порождающие и структурные паттерны | ⏳ запланировано |

## Технологии

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-latest-239120?logo=csharp&logoColor=white)
![tests](https://img.shields.io/badge/tests-MSTest-blue)

- C# / .NET 10
- MSTest
- GitHub Actions (сборка и тесты на каждый push и Pull Request)
- PlantUML для диаграмм

## Необходимо установить

Нужны: [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0), `Git`, `VS Code`, `C# Dev Kit`, `PlantUML`.

```bash
git clone https://github.com/Allureeee/LogHub-workshop.git
cd LogHub-workshop/LogHub
dotnet build
dotnet test
```

## Структура репозитория

```
LogHub-workshop/
├── LogHub/              # проект LogHub (solution, код и тесты)
│   ├── LogHub/
│   └── LogHub.Tests/
├── docs/                # отчёты по лабораторным (Markdown + PlantUML)
├── DesignPatterns/      # учебные примеры преподавателя (только чтение)
└── .github/workflows/   # CI
```

Папка `DesignPatterns/` пришла из [учебного репозитория](https://github.com/clgray/Lecture_Examples) и не изменяется.

## Процесс работы

- `main` защищена: изменения попадают в неё только через Pull Request
- `develop` — основная ветка разработки
- каждая работа делается в ветке `feature/лр-N` и сдаётся Pull Request'ом в `develop`
- CI должен быть зелёным, предупреждения компилятора считаются ошибками
