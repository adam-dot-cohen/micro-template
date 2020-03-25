az extension add --name aks-preview

# create service principal for cluster
#az ad sp create-for-rbac --skip-assignment --name sp-laso-dev-insights-kc | Out-File cluster-credential.json

$config = get-content .\cluster-credential.json | ConvertFrom-json

# create Kubernetes Cluster
az aks create --resource-group rg-laso-dev-insights --node-resource-group rg-laso-dev-insights-kc  --name kc-laso-dev-insights --node-count 1 --enable-addons monitoring --generate-ssh-keys --service-principal $config.appId --client-secret $config.password

# install kubectl cli
az aks install-cli

# get credentials
az aks get-credentials --resource-group rg-laso-dev-insights --name kc-laso-dev-insights

# verify connection to cluster
kubectl get nodes

