# Guia de integracao

Pagina unica para os outros tres modulos. Ninguem precisa ler meu codigo.

## Regra geral

Meu modulo nao conhece ninguem. Ele avisa o que aconteceu e expoe contratos. Voces escutam ou chamam.

---

## Para o modulo de Sistemas de Eventos

Escolha uma das tres portas, a que combinar com o que voce construiu.

**Porta 1, evento C# direto** quando voce tem a referencia do objeto:

```csharp
var vida = jogador.GetComponent<HealthComponent>();
vida.Model.Changed += c => Debug.Log($"vida {c.Current}/{c.Max}");
vida.Model.Died += () => Debug.Log("morreu");
```

**Porta 2, canal ScriptableObject** quando voce nao conhece o objeto:

```csharp
[SerializeField] HealthEventChannel canal;

void OnEnable()  => canal.Died += AoMorrer;
void OnDisable() => canal.Died -= AoMorrer;

void AoMorrer(GameObject quem) { /* ... */ }
```

Crie o asset em Assets, botao direito, Create, Game, Health, Event Channel. Arraste o mesmo asset no `HealthComponent` e no seu ouvinte.

**Porta 3, seu proprio barramento.** Se voce fez um EventBus proprio, escreva uma ponte de dez linhas assinando a porta 1 e republicando no seu formato. Nao preciso mudar nada do meu lado.

---

## Para o modulo de Salvamento e Carregamento

Use a interface, nao a classe:

```csharp
// salvar
var vida = jogador.GetComponent<IHealthSnapshot>();
HealthState estado = vida.Capture();
string json = JsonUtility.ToJson(estado);

// carregar
var estado = JsonUtility.FromJson<HealthState>(json);
vida.Restore(estado);
```

`HealthState` tem so dois campos int, `current` e `max`.

`Restore` dispara o evento de mudanca, entao a barra de vida se atualiza sozinha depois do load. Voce nao precisa avisar a UI.

---

## Para o modulo de Inventario com UI

**Pocao de cura:**

```csharp
if (usuario.TryGetComponent(out IHealable curavel))
    curavel.Heal(quantidade);
```

**Item que aumenta vida maxima:**

```csharp
vida.SetMaxHealth(150, keepRatio: true);
```

`keepRatio` true mantem a proporcao, ou seja, quem estava com metade continua com metade. False mantem o valor absoluto.

**Barra de vida na UI, sem escrever codigo:** no Inspector do `HealthComponent`, no evento `onNormalizedChanged`, arraste a Image da barra e escolha `Image.fillAmount`. Ele recebe um float de 0 a 1.

---

## O que eu garanto

- Vida nunca fica negativa nem passa do maximo.
- `Died` dispara uma unica vez por morte.
- Curar quem esta morto nao faz nada. Para ressuscitar, use `Revive`.
- Todo caminho de alteracao dispara `Changed`, inclusive o `Restore` do save.

## O que eu nao faco

Nao tenho invencibilidade temporaria, regeneracao ao longo do tempo, nem tipos de dano. Se algum de voces precisar, me avise que entra como extensao.
