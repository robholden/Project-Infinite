# Snow Capture

## Frontend

1. Install [Node.js](https://nodejs.org/en/download/)
2. Install Angular CLI `npm install -g @angular/cli`
3. Install dependencies

`cd Frontend/Desktop & npm i`
`cd Frontend/Mobile & npm i`
`cd Frontend/Shared & npm i`

**Start**

`cd Frontend/Desktop`
`npm i`
`ng serve --o`

## Backend

1. Install **.NET 3.1** & **.NET 6.0** at [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Install [Tye](https://github.com/dotnet/tye/blob/master/docs/getting_started.md)
3. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
4. Create user-secrets

`cd Backend/Library/Library.Service`
`dotnet user-secrets set "Shared:JwtIssuerKey" "A000A0000000AA000A0AA0000AA0A"`
`dotnet user-secrets set "Shared:ReCaptchaSecretKey" "your-key"`

**Start**
There are 2 tye configs for developing located at `Backend/.tye`

1. **all**: All depedencies and microservices are ran
2. **partial**: Project depedencies are ran (mssql, rabbitmq, etc.) and microservices are to be ran via VS debugger

`cd Backend/.tye/all`
`tye run --watch`

**Running a local SMTP server**
In order to send/receive emails you will need an smtp server

`dotnet tool install -g Rnwood.Smtp4dev --version "3.1.0-\*"`
`smtp4dev`
