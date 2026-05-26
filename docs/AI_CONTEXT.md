# vsHelp - AI Context

## Sobre o Projeto

O vsHelp é uma aplicação desktop interna desenvolvida para automatizar:

* restauração de backups
* instalações
* gerenciamento técnico
* processos operacionais internos

O projeto original foi desenvolvido em:

* WinForms
* DevExpress

Atualmente está em processo de modernização gradual para:

* WPF
* .NET 8
* arquitetura desacoplada
* UX/UI moderna

---

## Objetivo da Refatoração

Modernizar a aplicação mantendo:

* backend funcional existente
* compatibilidade operacional
* simplicidade de manutenção

Prioridades:

1. estabilidade
2. simplicidade
3. UX moderna
4. performance
5. baixo acoplamento

---

## Stack Atual

* .NET 8
* WPF
* CommunityToolkit.Mvvm
* XAML puro
* Sem DevExpress novo
* Sem MaterialDesignInXaml
* Sem frameworks visuais pesados

---

## Estrutura da Solução

* vsHelp.Core
* vsHelp.Application
* vsHelp.Infrastructure
* vsHelp.UI.WPF

---

## Diretrizes Gerais

* Evitar overengineering
* Evitar dependências desnecessárias
* Preferir soluções simples
* Priorizar legibilidade
* Priorizar manutenção fácil
* Evolução incremental
* Refatoração gradual do legado

---

## Regras Importantes

* NÃO modificar o projeto WinForms legado sem necessidade
* NÃO criar arquiteturas complexas
* NÃO adicionar bibliotecas externas sem necessidade
* NÃO misturar lógica de negócio com UI
* NÃO criar God Classes
* NÃO utilizar static desnecessariamente

---

## Objetivo Visual

A interface deve seguir um estilo moderno semelhante a:

* Linear
* Notion
* ClickUp
* Raycast
* Discord

Características desejadas:

* dark theme moderno
* minimalismo
* layout limpo
* boa hierarquia visual
* UX fluida
* aparência premium
