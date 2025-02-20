# MoneyMan API

MoneyMan is a .NET Core API designed to help you manage your monthly bills more effectively. It predicts due dates for your bills each month, considering weekends and bank holidays to ensure timely payments.

## Features

- **Bill Date Prediction**: Uses advanced algorithms to estimate bill due dates.
- **Weekend Consideration**: Adjusts due dates when bills fall on weekends.
- **Bank Holiday Adjustment**: Modifies due dates for recognized holidays.
- **Flexible Configuration**: Supports different billing cycles and payment frequencies.
- **User-Friendly API**: Provides endpoints to manage bills, view predictions, and configure settings.

## Getting Started

### Prerequisites

Ensure you have the following installed on your machine:

- [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)
- [Pre-commit](https://pre-commit.com/) (for Git hooks)
- [Playwright](https://playwright.dev/dotnet/) (for API testing)

### Clone the Repository

```sh
git clone https://github.com/halemiles/moneyman-api.git
cd moneyman-api
```

### Install Dependencies

```sh
dotnet restore
```

### Configure Pre-commit Hooks

To enforce code quality and linting, install `pre-commit` hooks:

```sh
pre-commit install
```

### Running the Application

#### Using .NET CLI

```sh
dotnet build

dotnet run
```

#### Using Docker

```sh
docker build -t moneyman-api .
docker run -p 5000:5000 moneyman-api
```

### Running Tests

#### Unit Tests

```sh
dotnet test
```

#### API Tests with Playwright

Ensure Playwright dependencies are installed:

```sh
playwright install
```

Run Playwright tests:

```sh
dotnet test --filter Category=Playwright
```

## Contributing

1. Fork the repository.
2. Create a new branch.
3. Commit your changes.
4. Push to your branch and submit a Pull Request.

## License

This project is licensed under the MIT License.

