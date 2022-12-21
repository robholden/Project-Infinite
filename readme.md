# Project Infinite

A never ending project I keep maintaining for fun!

## Frontend

Stack: `Angular 15` for desktop and `Ionic 6` for Mobile/PWA

1. Install [Node.js](https://nodejs.org/en/download/)
2. Install Angular CLI `npm install -g @angular/cli`
3. Install dependencies

```
cd Frontend/Desktop & npm i
cd Frontend/Ionic & npm i
cd Frontend/Shared & npm i
```

#### Start

```
cd Frontend/Desktop
```

```
npm i
```

```
ng serve --o
```

## Backend

Stack: `.net7`, `RabbitMQ` (via `MassTransit`), `EF Core 7`

1. Install **.NET 7.0** at [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Install [Tye](https://github.com/dotnet/tye/blob/master/docs/getting_started.md)
3. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
4. Create user-secrets

```
cd Backend/Library/Library.Service
```

```
dotnet user-secrets set "Shared:JwtIssuerKey" "A000A0000000AA000A0AA0000AA0A"
```

```
dotnet user-secrets set "Shared:ReCaptchaSecretKey" "your-key"
```

#### Running

Use `tye` to run dependencies locally with Docker -> `.dev/readme.md`

#### Running a local SMTP server

In order to send/receive emails you will need a local smtp server such as [https://github.com/ChangemakerStudios/Papercut-SMTP](Papercut-SMTP)
