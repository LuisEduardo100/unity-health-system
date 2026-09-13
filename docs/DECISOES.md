# Decisoes de projeto

Registro do que foi escolhido e por que. Serve para eu explicar o trabalho na apresentacao.

Formato: decisao, alternativa descartada, motivo.

---

## D01 Nucleo da vida em C# puro, sem UnityEngine

**Escolha:** a classe `Health` nao tem nenhum `using UnityEngine`.

**Alternativa descartada:** colocar a logica direto num `MonoBehaviour`.

**Motivo:** MonoBehaviour so existe preso a um GameObject numa cena rodando. Logica dentro dele so da para testar entrando em play e olhando com o olho. Com C# puro, os testes rodam em milissegundos no Test Runner sem abrir cena, e a mesma regra serve para jogador, inimigo ou objeto destrutivel.

---

## D02 MonoBehaviour como adaptador, nao como dono da regra

**Escolha:** `HealthComponent` nao decide nada. Ele cria um `Health`, escuta os eventos dele e repassa para o Inspector e para o canal.

**Alternativa descartada:** o componente calcular dano e clamp por conta propria.

**Motivo:** padrao adaptador. A borda do sistema muda com frequencia (UI nova, efeito de particula, som). O miolo nao muda. Separar os dois evita que mexer na apresentacao quebre a regra.

---

## D03 Vida em numero inteiro

**Escolha:** `int`, nao `float`.

**Alternativa descartada:** float com casas decimais.

**Motivo:** float acumula erro de arredondamento, entao comparar `vida == 0` vira armadilha. Com int, morrer e exatamente chegar a zero. Se o jogo precisar de dano fracionado depois, a conversao acontece de fora, arredondando antes de chamar `TakeDamage`.

---

## D04 Caminho unico de escrita

**Escolha:** todo dano, cura, revive e restore passam pelo metodo privado `Apply`.

**Alternativa descartada:** cada metodo publico mexer no campo `current` e disparar seus proprios eventos.

**Motivo:** com varios caminhos de escrita, cedo ou tarde alguem esquece de disparar um evento ou de aplicar o clamp, e a barra de vida dessincroniza do estado real. Funil unico torna esse bug impossivel por construcao.

---

## D05 Tres formas de escutar, para nao apostar numa so

**Escolha:** o modulo publica em evento C# (`Health.Changed`), em UnityEvent do Inspector e num `ScriptableObject` de canal.

**Alternativa descartada:** escolher apenas uma.

**Motivo:** estou integrando as cegas com tres colegas. Evento C# serve para quem tem referencia ao objeto. UnityEvent serve para ligar UI sem escrever codigo. Canal ScriptableObject serve para quem nao conhece nem o objeto nem a cena. Custa pouco manter os tres e cobre qualquer decisao que eles tomem.

---

## D06 Canal de eventos em ScriptableObject, nao em singleton

**Escolha:** `HealthEventChannel` e um asset do projeto. Emissor e ouvinte arrastam o mesmo arquivo.

**Alternativa descartada:** um `GameManager` estatico com `Instance`.

**Motivo:** singleton cria dependencia escondida, ordem de inicializacao fragil e teste dificil. Com asset, a ligacao fica visivel no Inspector, da para ter varios canais (jogador, inimigo, chefe) e nenhum script conhece o outro.

**Cuidado registrado:** ScriptableObject sobrevive ao stop do editor. Por isso o canal limpa os assinantes em `OnDisable`, senao a sessao seguinte herda ouvintes mortos.

---

## D07 Estado de save como struct simples e serializavel

**Escolha:** `HealthState` com campos publicos `current` e `max`.

**Alternativa descartada:** entregar o objeto `Health` inteiro para o sistema de save.

**Motivo:** `JsonUtility` do Unity so enxerga campos publicos, nao propriedades, e nao serializa eventos. Alem disso, quem salva nao pode depender da minha implementacao interna: se eu mudar `Health` depois, o arquivo de save antigo continua valendo porque o contrato e o `HealthState`, nao a classe.

---

## D08 Interfaces como fronteira com os outros modulos

**Escolha:** `IDamageable`, `IHealable`, `IHealthSnapshot`.

**Alternativa descartada:** os colegas chamarem `HealthComponent` direto.

**Motivo:** interface e o contrato minimo. O modulo de inventario compila conhecendo so `IHealable`. Se amanha trocarmos a implementacao de vida, nada do lado dele muda. E o que permite plugar e desplugar.

---

## D09 Duas assemblies, com o nucleo proibido de ver o Unity

**Escolha:** `Game.Health.Core` com `noEngineReferences: true` e lista de referencias vazia, e `Game.Health.Unity` que referencia o nucleo.

**Alternativa descartada:** uma assembly so, ou deixar tudo no `Assembly-CSharp` padrao.

**Motivo:** a flag `noEngineReferences` faz o compilador recusar qualquer `using UnityEngine` dentro do nucleo. A decisao D01 deixa de ser disciplina e vira erro de compilacao. A lista de referencias vazia tem o mesmo efeito na outra direcao: meu modulo nao consegue importar codigo de inventario ou de save nem por acidente. De quebra, mudar um script meu recompila so este pacote, nao o projeto inteiro.

---

## D10 Distribuir como pacote UPM, nao como pasta de projeto

**Escolha:** repositorio com `package.json`, instalado via Install package from disk.

**Alternativa descartada:** um projeto Unity completo com a pasta `Assets`.

**Motivo:** cada um de nos trabalha no seu repositorio e o projeto do grupo puxa os quatro pacotes. Evita conflito de merge em cena e em ProjectSettings, que e a dor classica de Unity em equipe. Tambem me destrava: comeco agora sem depender do que os outros tres fizerem.

---

## D11 Projeto no disco do Windows

**Escolha:** codigo em `C:\Users\Notebook\unity-health-system`.

**Alternativa descartada:** dentro do sistema de arquivos do WSL.

**Motivo:** Unity e aplicativo Windows. Ler pasta do WSL passa por um protocolo de rede e deixa importacao de asset lenta, alem de confundir o detector de mudanca de arquivo do editor. O terminal Linux acessa o mesmo caminho por `/mnt/c` sem custo.

---

## D12 Git LFS e merge de YAML configurados desde o primeiro commit

**Escolha:** `.gitattributes` marcando cena e prefab como YAML e imagem e audio como LFS.

**Alternativa descartada:** configurar depois, quando doer.

**Motivo:** LFS so vale para arquivo commitado a partir do momento em que a regra existe. Se um asset pesado entrar antes, ele fica no historico do git para sempre. Configurar antes do primeiro commit e barato, corrigir depois exige reescrever historico.
