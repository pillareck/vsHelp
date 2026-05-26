# Arquitetura

## Objetivo

Separar responsabilidades do sistema para:

* facilitar manutenção
* reduzir acoplamento
* permitir evolução gradual
* melhorar testabilidade

---

## Estrutura

### vsHelp.Core

Responsável por:

* modelos
* contratos
* interfaces
* regras centrais

Não deve:

* acessar UI
* acessar banco diretamente
* depender de WPF

---

### vsHelp.Infrastructure

Responsável por:

* acesso MySQL
* arquivos
* compressão
* integração externa
* Google Drive
* IO

Não deve:

* conter lógica visual
* acessar Views

---

### vsHelp.Application

Responsável por:

* orquestração
* regras de aplicação
* casos de uso
* serviços principais

Exemplos:

* RestoreService
* BackupService
* InstallationService

---

### vsHelp.UI.WPF

Responsável por:

* interface
* navegação
* experiência visual
* binding
* interação com usuário

Não deve:

* conter regras complexas
* acessar banco diretamente

---

## Regras Técnicas

Preferir:

* async/await
* serviços pequenos
* responsabilidade única
* código incremental
* composição simples

Evitar:

* classes gigantes
* static excessivo
* Thread manual
* lógica na code-behind
* acoplamento forte

---

## Refatoração do Legado

A migração será:

* gradual
* incremental
* orientada por funcionalidades

Objetivo:

* reaproveitar backend existente inicialmente
* desacoplar aos poucos
* reduzir riscos operacionais

---

## Prioridades Atuais

1. estabilizar nova UI WPF
2. implementar fluxo de restauração
3. desacoplar backend
4. melhorar logs e progresso
5. substituir automações frágeis
6. melhorar arquitetura interna
