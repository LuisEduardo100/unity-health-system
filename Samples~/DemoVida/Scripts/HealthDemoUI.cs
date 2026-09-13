using UnityEngine;
using UnityEngine.UI;

namespace Game.Health.Demo
{
    /// <summary>
    /// Demo do modulo de vida. Ela nao contem regra nenhuma: so escuta o que o Health avisa
    /// e traduz para barra, numero e log na tela.
    /// </summary>
    public class HealthDemoUI : MonoBehaviour
    {
        [SerializeField] HealthComponent alvo;
        [SerializeField] Image preenchimento;
        [SerializeField] Text numero;
        [SerializeField] Text registro;

        [SerializeField] Button botaoDano;
        [SerializeField] Button botaoCura;
        [SerializeField] Button botaoMatar;
        [SerializeField] Button botaoReviver;
        [SerializeField] Button botaoSalvar;
        [SerializeField] Button botaoCarregar;

        const string Chave = "demo.vida";
        readonly System.Collections.Generic.Queue<string> linhas = new System.Collections.Generic.Queue<string>();

        // Start, nao Awake: garante que o Awake do HealthComponent ja rodou e o Model existe.
        void Start()
        {
            if (alvo == null) { Debug.LogError("HealthDemoUI sem alvo."); return; }

            alvo.Model.Changed += AoMudar;
            alvo.Model.Died += () => Log("morreu");
            alvo.Model.Revived += () => Log("reviveu");

            botaoDano.onClick.AddListener(() => alvo.TakeDamage(10));
            botaoCura.onClick.AddListener(() => alvo.Heal(15));
            botaoMatar.onClick.AddListener(() => alvo.TakeDamage(9999));
            botaoReviver.onClick.AddListener(() => alvo.Revive());
            botaoSalvar.onClick.AddListener(Salvar);
            botaoCarregar.onClick.AddListener(Carregar);

            Desenhar(alvo.Current, alvo.Max);
            Log("cena iniciada");
        }

        void OnDestroy()
        {
            if (alvo != null && alvo.Model != null) alvo.Model.Changed -= AoMudar;
        }

        void AoMudar(HealthChange c)
        {
            Desenhar(c.Current, c.Max);
            Log(c.IsHeal ? $"curou {c.Delta}" : $"tomou {-c.Delta}");
        }

        void Desenhar(int atual, int max)
        {
            float n = max <= 0 ? 0f : (float)atual / max;
            preenchimento.fillAmount = n;
            preenchimento.color = n > 0.5f ? new Color(0.30f, 0.78f, 0.35f)
                                : n > 0.25f ? new Color(0.95f, 0.75f, 0.18f)
                                            : new Color(0.85f, 0.26f, 0.26f);
            numero.text = $"{atual} / {max}";
        }

        /// <summary>Mostra na pratica o contrato que o modulo de save vai usar.</summary>
        void Salvar()
        {
            PlayerPrefs.SetString(Chave, JsonUtility.ToJson(alvo.Capture()));
            PlayerPrefs.Save();
            Log("salvou");
        }

        void Carregar()
        {
            if (!PlayerPrefs.HasKey(Chave)) { Log("nada salvo ainda"); return; }
            alvo.Restore(JsonUtility.FromJson<HealthState>(PlayerPrefs.GetString(Chave)));
            Log("carregou");
        }

        void Log(string s)
        {
            linhas.Enqueue(s);
            while (linhas.Count > 7) linhas.Dequeue();
            registro.text = string.Join("\n", linhas);
        }
    }
}
