Add Dashboard

```
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.4.0/aio/deploy/recommended.yaml
```

```
kubectl apply -f .\service-account.yaml
```

```
kubectl -n kubernetes-dashboard get secret $(kubectl -n kubernetes-dashboard get sa/admin-user -o jsonpath="{.secrets[0].name}") -o go-template="{{.data.token | base64decode}}"
```

Install Redis & RabbitMq
`helm repo add bitnami https://charts.bitnami.com/bitnami`
`helm repo update`
`helm install sc-redis bitnami/redis`
`helm install sc-rabbitmq bitnami/rabbitmq`

Apply manifests
`kubectl apply -f data/mssql-pvc.yaml`
`kubectl apply -f data/secrets.yaml`
`kubectl apply -f data/mssql.yaml`

`kubectl apply -f config-map-map.yaml`
`kubectl apply -f secrets.yaml`
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

Redeploying Pods

```
kubectl rollout restart deployment gateway-api
kubectl rollout restart deployment comms-api
kubectl rollout restart deployment identity-api
kubectl rollout restart deployment content-api
kubectl rollout restart deployment reports-api
kubectl rollout restart deployment sockets-api
kubectl rollout restart deployment web-ui
```

Copying local image to minikube

```
minikube image load image:tag
```

Gettings logs

```
kubectl logs pod-name --since-time=2021-12-31T00:00:00Z > pod-logs.txt
```
