## Despliegue en Kubernetes

Se incluyen manifiestos (`k8s/`) para Orders API como referencia de diseño: `Deployment` (límites de CPU/memoria, y `replicas: 1` — justificado más abajo), `Service` tipo `ClusterIP`, `ConfigMap` para configuración no sensible, `Secret` para credenciales, y un `PersistentVolumeClaim` para la base SQLite.

**Por qué `replicas: 1`:** SQLite no soporta bien escrituras concurrentes desde múltiples pods sobre el mismo archivo. Escalar horizontalmente este servicio requeriría antes migrar a PostgreSQL (ver "Qué haría distinto"), donde sí tendría sentido subir las réplicas y usar un PVC compartido o una base gestionada externa.

**Cómo desplegaría el sistema completo:**

1. **RabbitMQ**: usaría el chart oficial de Helm (`bitnami/rabbitmq`) en vez de manifiestos manuales, para obtener clustering y persistencia ya resueltos.
2. **Orders API e Inventory Worker**: mismo patrón que el manifiesto incluido (Deployment + ConfigMap + Secret + PVC), uno por servicio, cada uno con su propio PVC para su base SQLite independiente (o migrados a PostgreSQL, ver arriba).
3. **Frontend**: un Deployment simple sirviendo el build estático vía nginx (igual que en Docker), sin necesidad de PVC.
4. **Ingress**: un `Ingress` (nginx-ingress o similar) enrutando `/api` hacia `orders-api-service` y `/` hacia el frontend, exponiendo un solo punto de entrada externo.
5. **Secrets reales**: en un clúster real, las credenciales de RabbitMQ y cualquier otro secreto vendrían de un gestor externo (Sealed Secrets, External Secrets Operator, o el proveedor cloud), no como `stringData` plano en el manifiesto.
6. **Namespaces**: un namespace `orderflow` dedicado, para aislar el proyecto de otros workloads del clúster.

No se incluyó un clúster funcional ni manifiestos para los demás servicios por priorización de tiempo — este manifiesto de Orders API sirve como muestra representativa del patrón que se replicaría en los demás.
