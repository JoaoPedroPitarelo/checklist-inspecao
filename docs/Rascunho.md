# Rascunho - Prova Técnica SystemFy Dev .NET Júnior
___

### Negócio:

#### **Passo Certo** rede fictícia de lojas de calçados

- 11 lojas espalhadas por 1 estado.
- Todas as lojas vendem os mesmos produtos.

#### Como a rede está organizada:

- ###### 3 regiões, Litoral, Metropolitana e Interior
- ###### Cada loja pertence a uma _única região_
- ###### Cada loja tem 1 gerente, que é responsável pela loja.

#### Qual o problema que querem resolver?

- CheckList preenchida em papel ou planilhas soltas
- Uma nova visita de inspeção é as vezes anotada acima da anterior, perdendo o histórico.
- Para saber como a rede ou região está, alguém junta as planilhas à mão e as comparada, muitas vezes, com erros.
- Não há controle de quem pode ou não ver as informações da inspeção

#### O quê o cliente quer como solução?

- Uma aplicação web, usável no celular, para o supervisor registrar as inspeções.
- Todas as visitas guardadas mantendo um histórico sem apagar a anterior.
- O percentual (percentual do que???) calculado automaticamente.
- Um dashboard (painel de indicadores) montado a partir das próprias inspeções.
- Cada supervisor vendo só as lojas das suas regiões.

#### O que é uma inspeção?

A inspeção é a **avaliação** que o supervisor faz **durante a visita** a uma loja. Ela segue sempre a mesma lista de ponto a verificar, chamada de checklist.

#### Pessoas e perfis

- ###### Gerente da loja *(não tem acesso ao sistema)*
- ###### Supervisor regional (perfil ==🟣supervisor==)
- ###### Diretora Comercial (perfil ==🟣comercial==)
- ###### Administrador (perfil ==🟣administrador== **`bônus`**)

##### O que cada perfil pode fazer?


| O que pode fazer                              |        Supervisor         |  Diretoria  |  Administrador  |
| :-------------------------------------------- | :-----------------------: | :---------: | :-------------: |
| Ver lojas, histórico, detalhe e dashboard     | ==🟠só das suas regiões== | ==🟢todas== |   ==🟢todas==   |
| Criar inspeção                                |         ==🟢Sim==         |      -      |        -        |
| Editar ou concluir inspeção em rascunho       |   ==🟠só as que criou==   |      -      |        -        |
| Manter cadastros e reabrir inspeção concluída |             -             |      -      | Sim ==🟣bônus== |
