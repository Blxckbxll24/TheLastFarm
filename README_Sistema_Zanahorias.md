# 🥕 Sistema de Zanahorias - Moneda del Juego

## 📋 Resumen del Sistema

El sistema de zanahorias convierte los cultivos cosechados en una moneda recolectable que el jugador puede recoger tocándolas.

### ✨ **Características Principales:**

1. **🌱 Auto-generación**: Las zanahorias se crean automáticamente al cosechar cultivos
2. **🎾 Físicas realistas**: Se lanzan con efecto de arco desde el cultivo cosechado
3. **💫 Efectos visuales**: Flotación y rotación continua
4. **💰 Sistema de monedas**: Se integra con un sistema de monedas global
5. **⏰ Auto-destrucción**: Desaparecen después de un tiempo si no se recogen

## 🎮 Configuración Rápida

### 1. Preparar el Prefab de Zanahoria
1. **Crear GameObject** con tu sprite de zanahoria
2. **Agregar componentes**:
   - `Zanahoria.cs` script
   - `CircleCollider2D` (marcado como **Trigger**)
   - `Rigidbody2D` (opcional, se agrega automáticamente)
3. **Configurar Collider**: 
   - `Is Trigger` = ✅
   - Ajustar el `Radius` para que sea fácil de recoger

### 2. Configurar el Jugador
1. **Agregar `SistemaMonedas.cs`** al GameObject del jugador
2. **Configurar en el inspector** los valores deseados
3. **Opcional**: Crear UI Text para mostrar las monedas

### 3. Configurar CultivoManager
1. **Abrir el Inspector** del CultivoManager
2. **En la sección "🥕 SISTEMA DE ZANAHORIAS"**:
   - Asignar el **Prefab Zanahoria**
   - Configurar cantidad y efectos

## ⚙️ Configuración Detallada

### Zanahoria.cs
```
💰 Configuración de Moneda:
├── Valor: 1 (monedas por zanahoria)
├── Velocidad Flotación: 1.0f
├── Amplitud Flotación: 0.2f
├── Velocidad Rotación: 90°/seg
├── Efecto Flotación: ✓
└── Efecto Rotación: ✓

⏱️ Auto-destrucción:
├── Auto-destruirse: ✓
├── Tiempo Vida: 30 segundos
└── Tiempo Parpadeo: 5 segundos
```

### SistemaMonedas.cs
```
💰 Sistema de Monedas:
├── Monedas Actuales: 0
├── Monedas Máximas: 999999
├── Mostrar Debug Info: ✓
└── Formato Texto: "🥕 {0}"

📊 UI (Opcional):
└── Texto Monedas: (UI Text para mostrar)
```

### CultivoManager.cs - Nuevas Configuraciones
```
🥕 SISTEMA DE ZANAHORIAS:
├── Prefab Zanahoria: (asignar prefab)
├── Valor Zanahoria: 1 moneda
├── Cantidad Por Cosecha: 1 zanahoria
├── Fuerza Lanzamiento: 5.0f
├── Altura Lanzamiento: 2.0f
├── Efecto Lanzamiento: ✓
└── Debug Zanahorias: ✓
```

## 🎯 Funcionamiento del Sistema

### 🌾 Al Cosechar un Cultivo:
1. **Se detecta** que el cultivo está maduro
2. **Se lanzan zanahorias** desde el centro del cultivo
3. **Efecto físico**: Las zanahorias vuelan en arco
4. **Caen al suelo** y empiezan a flotar/rotar

### 🥕 Comportamiento de las Zanahorias:
- **Flotación suave** arriba y abajo
- **Rotación continua** en el eje Z
- **Colisión**: Solo con el jugador (trigger)
- **Parpadeo**: 5 segundos antes de desaparecer
- **Auto-destrucción**: A los 30 segundos

### 💰 Al Recoger una Zanahoria:
1. **Detección automática** cuando el jugador las toca
2. **Se agrega** el valor al sistema de monedas
3. **Efecto visual**: Escala hacia arriba y desvanece
4. **Sonido**: Opcional, si está configurado
5. **Log**: Información en la consola

## 🎮 Controles y Uso

### Controles del Jugador
- **Movimiento normal**: Simplemente toca las zanahorias para recogerlas
- **No se requiere input especial**: La recolección es automática

### Controles Debug (SistemaMonedas)
- **M**: Agregar 10 monedas (solo en editor)
- **N**: Gastar 5 monedas (solo en editor)

### Controles Debug (CultivoManager)
- **C**: Cosechar cultivo donde esté el cursor
- **Consola**: Logs detallados del lanzamiento de zanahorias

## 🔧 Métodos Importantes

### Zanahoria.cs
```csharp
// Configurar valor manualmente
zanahoria.SetValor(int valor);

// Obtener valor actual
int valor = zanahoria.GetValor();

// Configurar efectos
zanahoria.ConfigurarEfectos(bool flotacion, bool rotacion);
```

### SistemaMonedas.cs
```csharp
// Agregar monedas
bool exito = sistemaMonedas.AgregarMonedas(int cantidad);

// Gastar monedas
bool exito = sistemaMonedas.GastarMonedas(int cantidad);

// Verificar monedas
bool tiene = sistemaMonedas.TieneSuficientesMonedas(int cantidad);

// Obtener cantidad actual
int monedas = sistemaMonedas.MonedasActuales;

// Acceso estático global
SistemaMonedas.AgregarMonedasGlobal(10);
int total = SistemaMonedas.ObtenerMonedasActuales();
```

## 📊 Información Visual

### En el Editor (Scene View):
- **Zanahoria**: Muestra valor y estado (🥕 $1)
- **SistemaMonedas**: Información de monedas actuales
- **CultivoManager**: Debug de lanzamiento de zanahorias

### En la Consola:
- **🥕 LANZANDO**: Cuántas zanahorias desde qué posición
- **🚀**: Fuerza aplicada a cada zanahoria
- **✅**: Confirmación de creación exitosa
- **💰 MONEDAS GANADAS**: Al recoger zanahorias

## 🎨 Personalización Avanzada

### Efectos de Lanzamiento
Puedes ajustar cómo vuelan las zanahorias:
- **Fuerza Lanzamiento**: Qué tan lejos vuelan
- **Altura Lanzamiento**: Qué tan alto van
- **Variación de Ángulo**: Aleatoriedad del lanzamiento

### Efectos Visuales
- **Flotación**: Movimiento vertical suave
- **Rotación**: Giro continuo
- **Parpadeo**: Advertencia antes de desaparecer
- **Escala al recoger**: Efecto de crecimiento y desvanecimiento

### Sistema de Monedas
- **Límite máximo**: Configurable
- **Persistencia**: Se guarda automáticamente
- **Eventos**: Para triggers de otros sistemas
- **UI**: Integración opcional con interfaz

## 🐛 Troubleshooting

1. **Las zanahorias no aparecen**:
   - Verificar que el prefab esté asignado en CultivoManager
   - Comprobar que `Efecto Lanzamiento` esté activado

2. **No se pueden recoger**:
   - Verificar que el Collider2D sea **Trigger**
   - Asegurar que el jugador tenga tag "Player"

3. **No se agregan monedas**:
   - Verificar que haya un SistemaMonedas en la escena
   - Comprobar los logs en la consola

4. **Zanahorias vuelan muy lejos/cerca**:
   - Ajustar `Fuerza Lanzamiento` y `Altura Lanzamiento`
   - Modificar la `Gravity Scale` del Rigidbody2D

## 📝 Lista de Verificación

- [ ] Prefab de zanahoria creado con sprite
- [ ] Script Zanahoria.cs agregado al prefab
- [ ] CircleCollider2D configurado como Trigger
- [ ] SistemaMonedas.cs agregado al jugador
- [ ] Prefab asignado en CultivoManager
- [ ] Valores configurados en el inspector
- [ ] Cultivos plantados y maduros para probar
- [ ] Tag "Player" asignado al jugador

¡El sistema está listo! Planta cultivos, espera a que maduren, coséchalos con C, y recoge las zanahorias que aparezcan.