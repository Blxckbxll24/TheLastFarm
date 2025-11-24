# 🎮 Sistema de Vida del Jugador y Controlador de Zombies

## 📋 Resumen de Funcionalidades

### 🫀 Sistema de Vida del Jugador
- **Salud configurable**: Salud máxima y actual del jugador
- **Sistema de inmunidad**: Tiempo de inmunidad después de recibir daño
- **Efectos visuales**: Parpadeo rojo durante la inmunidad
- **Sistema de muerte**: El jugador se vuelve gris y no puede moverse
- **Detección de colisión**: Recibe daño automáticamente al tocar enemigos
- **🛡️ Protección durante ataque**: No recibe daño mientras ataca

### 🧟 Controlador de Zombies
- **Spawn masivo**: Genera 10 zombies de una vez
- **Posicionamiento inteligente**: Los zombies aparecen en el suelo y no se superponen
- **Sistema de respawn**: Opcional, mantiene un número constante de zombies
- **Configuración flexible**: Salud, velocidad y daño configurables
- **Visualización debug**: Muestra información en tiempo real
- **💥 Sistema de retroceso**: Los zombies retroceden al ser atacados
- **🛡️ No atacan durante el retroceso del jugador**: Respetan cuando el jugador está atacando

## 🚀 Configuración Rápida

### 1. Configurar el Jugador
1. **MovimientoJugador.cs** ya está actualizado con:
   - Variables de vida en el inspector
   - Sistema de inmunidad temporal
   - Efectos visuales de daño y muerte

### 2. Configurar Zombies
1. **Crear un prefab de zombie**:
   - GameObject con SpriteRenderer
   - Collider2D (marcado como Trigger para daño)
   - Rigidbody2D
   - **ControladorEnemigo.cs** script
   - Tag: "Enemy"

2. **Crear el controlador**:
   - GameObject vacío en la escena
   - Agregar **ControladorZombies.cs**
   - Asignar el prefab del zombie
   - Configurar parámetros en el inspector

### 3. Configurar Layers (Opcional)
- Crear layer "Enemy" para los zombies
- Asignar en capaEnemigos del jugador

## 🎛️ Configuración del Inspector

### MovimientoJugador.cs - Sistema de Vida
```
💖 Sistema de Vida:
├── Salud Maxima: 100
├── Tiempo Inmunidad: 1.0f
└── Jugador Sprite: (asignar automáticamente)
```

### ControladorZombies.cs
```
🧟 CONFIGURACIÓN DE SPAWN:
├── Prefab Zombie: (asignar tu prefab)
├── Cantidad Zombies: 10
├── Rango Spawn: 10.0f
├── Tiempo Entre Spawns: 0.2f
├── Spawnear Al Inicio: ✓
├── Capa Suelo: Everything (-1) ⭐ MEJORADO
├── Altura Raycast: 10.0f ⭐ NUEVO
├── Distancia Raycast: 20.0f ⭐ NUEVO
└── Offset Suelo: 1.0f ⭐ NUEVO

🎯 CONFIGURACIÓN DE ZOMBIES:
├── Salud Zombie: 30
├── Velocidad Zombie: 2.5f
└── Daño Zombie: 20

🔄 SISTEMA DE RESPAWN:
├── Respawn Activado: ⬜ (opcional)
├── Tiempo Respawn: 5.0f
└── Maximo Zombies Vivos: 10
```

### ControladorEnemigo.cs - Nuevas Variables
```
Estadísticas:
├── Salud: 30
├── Velocidad Movimiento: 2.5f
└── Daño: 20 ⭐ (NUEVO)

⚡ Sistema de Retroceso: ⭐ (NUEVO)
├── Fuerza Retroceso: 8.0f
├── Tiempo Retroceso: 0.5f
└── Puede Recibir Retroceso: ✓
```

## 🎮 Controles y Funcionalidades

### Controles del Juego
- **Movimiento**: Flechas/WASD
- **Salto**: Espacio
- **Ataque**: Click izquierdo

### Controles Debug (ControladorZombies)
- **Z**: Mostrar información de zombies
- **X**: Spawnear más zombies manualmente

## 🔧 Métodos Públicos Importantes

### MovimientoJugador
```csharp
// Recibir daño
jugador.RecibirDaño(int cantidad);

// Curar
jugador.Curar(int cantidad);

// Estado
bool estaMuerto = jugador.EstaMuerto();
bool esInmune = jugador.EsInmune();
bool estaAtacando = jugador.EstaAtacando(); // ⭐ NUEVO
int salud = jugador.GetSaludActual();
```

### ControladorZombies
```csharp
// Spawn manual
controlador.SpawnearZombies();

// Control de respawn
controlador.ActivarRespawn();
controlador.DesactivarRespawn();

// Información
int vivos = controlador.GetZombiesVivos();
bool spawneando = controlador.EstaSpawneando();

// Destruir todos
controlador.DestruirTodosLosZombies();
```

## 🎨 Efectos Visuales

### Jugador
- **Daño**: Parpadeo rojo durante inmunidad
- **Muerte**: Se vuelve gris y semi-transparente
- **UI Debug**: Información de salud en el editor

### Zombies
- **Spawn Visual**: Los zombies aparecen gradualmente
- **Gizmos**: Área de spawn y conexiones en el editor
- **Debug Info**: Contador en tiempo real

## 📝 Notas Técnicas

### Detección de Daño
- Los zombies dañan al jugador cuando:
  1. Están en rango de ataque (distanciaAtaque)
  2. El jugador no está en inmunidad
  3. **🛡️ El jugador NO está atacando** ⭐ NUEVO
  4. No está muerto

- **💥 Sistema de Retroceso**: ⭐ NUEVO
  - Los zombies retroceden al recibir daño
  - Fuerza y duración configurables
  - No pueden atacar durante el retroceso

### Colisiones
- **Jugador**: OnTriggerEnter2D para recibir daño
- **Zombies**: IniciarAtaque() para causar daño

### Performance
- Lista de zombies se limpia automáticamente
- Spawn asíncrono para evitar lag
- Sistema de respawn configurable

## 🐛 Troubleshooting

1. **Solo sale un zombie o pocos zombies**:
   - **Solución inmediata**: Presionar **C** para forzar spawn completo
   - Verificar que `Cantidad Zombies` esté en 10
   - Revisar la consola para errores de spawn
   - Si cambias `Tiempo Entre Spawns` a valores altos, usar la tecla C

2. **Zombies aparecen dentro del tilemap**:
   - Ajustar `Offset Suelo` (por defecto 1.0f)
   - Verificar que el tilemap tenga colliders
   - Configurar `Capa Suelo` para incluir el layer del tilemap
   - Aumentar `Altura Raycast` si es necesario

3. **Zombies no spawean**:
   - Verificar que el prefab esté asignado
   - Comprobar que `Capa Suelo` no esté en 0 (Nothing)
   - Revisar la consola para logs de debug

4. **Jugador no recibe daño**:
   - Verificar que los zombies tengan tag "Enemy"
   - Comprobar configuración de layers

5. **Zombies no se mueven**:
   - Verificar que el prefab tenga ControladorEnemigo
   - Asegurar que el jugador tenga tag "Player"

## 🔧 Configuración Recomendada para Tilemaps

Para evitar que zombies aparezcan dentro de tilemaps:

1. **Capa Suelo**: Seleccionar solo el layer de tu tilemap
2. **Offset Suelo**: 1.0f o mayor
3. **Altura Raycast**: 10.0f (para buscar desde arriba)
4. **Distancia Raycast**: 20.0f (suficiente para encontrar suelo)

## 🎮 Controles Debug Mejorados

- **Z**: Información completa de zombies y configuración
- **X**: Spawn manual de zombies (normal)
- **C**: Forzar spawn completo (asegura que salgan todos) ⭐ NUEVO
- **G (en Scene View)**: Visualizar raycasts de debug en tiempo real

## ✅ Lista de Verificación

- [ ] Prefab de zombie creado con todos los componentes
- [ ] ControladorZombies configurado en la escena
- [ ] Tags "Player" y "Enemy" asignados correctamente
- [ ] Layers configurados (opcional)
- [ ] Parámetros ajustados en el inspector
- [ ] Prueba de spawn funcionando (tecla X)
- [ ] Sistema de daño funcionando

¡El sistema está listo para usar! Los zombies aparecerán automáticamente al iniciar el juego y el jugador podrá recibir daño y morir sin ser destruido.