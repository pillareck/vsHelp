# Known Issues

## WPF

* Evitar dependências visuais complexas desnecessárias
* Evitar overengineering em XAML
* Priorizar simplicidade

---

## MaterialDesignInXaml

Problemas identificados:

* incompatibilidades frequentes entre versões
* APIs inconsistentes
* conflitos em ResourceDictionary
* alto custo de manutenção

Decisão:

* NÃO utilizar no projeto atual

---

## Legado WinForms

Pontos críticos identificados:

* uso de Thread manual
* SendKeys frágeis
* lógica acoplada à UI
* classes utilitárias gigantes
* static excessivo

A migração será gradual para evitar impacto operacional.

---

## Refatoração

Regras:

* evitar grandes reescritas de uma vez
* validar incrementalmente
* priorizar estabilidade
* manter funcionamento operacional
