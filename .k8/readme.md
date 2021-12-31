Install Redis & RabbitMq
`helm repo add bitnami https://charts.bitnami.com/bitnami`
`helm repo update`
`helm install sc-redis bitnami/redis`
`helm install sc-rabbitmq bitnami/rabbitmq`

Apply manifests
`kubectl apply -f data/mssql-pvc.yaml`
`kubectl apply -f data/mssql-secrets.yaml`
`kubectl apply -f data/mssql.yaml`

`kubectl apply -f site-config-map.yaml`
`kubectl apply -f site-secrets.yaml`
`kubectl apply -f gateway-api.yaml`
`kubectl apply -f identity-api.yaml`

Add dev cert
`choco install mkcert`
`mkcert snowcapture.com "*.snowcapture.com" localhost 127.0.0.1 ::1`
`kubectl -n kube-system create secret tls mkcert --key snowcapture.com+4-key.pem --cert snowcapture.com+4.pem`

Enable Ingress
`minikube addons configure ingress` -> `kube-system/mkcert`
`minikube addons disable ingress`
`minikube addons enable ingress`
`kubectl apply -f ingress.yaml`

Start tunnel for access
`minikube tunnel`
Browse [snowcapture.dev]`snowcapture.dev`

Generating a secret

```
base64-string input:"YOUR-VALUE"
```

Redeploy Pod

```
kubectl rollout restart deployment my-deployment
```
