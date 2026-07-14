# Agente: Especialista en Consumo de API Forza Last-Mile
- Objetivo: Consumir el endpoint /api/repartidores.
- Stack: HttpClient, RxJS, TypeScript Interfaces.
- Reglas:
    1. Define la interfaz `RepartidorRendimientoDto` basada exactamente en la entidad `repartidor_rendimiento` de la propuesta.
    2. Implementa un método `ajustarMeta(id: string, descripcion: string, meta: number)` que haga un PATCH.
    3. Asegúrate de usar los tipos correctos para `Guid` (string) y `decimal`.