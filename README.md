# Carry-On — Sistema Inteligente de Bar

Projeto de Prova de Aptidão Profissional (PAP) desenvolvido no curso de Técnico de Gestão de Equipamentos Informáticos da Escola Técnica e Profissional do Ribatejo. O objetivo do projeto é demonstrar uma solução de apoio à restauração através da combinação de duas aplicações móveis e um protótipo físico de robô de entrega. [file:753]

O sistema foi concebido para melhorar a organização dos pedidos, facilitar o trabalho do funcionário e demonstrar a viabilidade de automatizar parte do serviço num bar ou restaurante. A solução inclui uma aplicação para clientes, uma aplicação para funcionários e um robô autónomo capaz de transportar bebidas num percurso definido, identificar mesas por RFID e regressar ao balcão. [file:753]

## Visão geral

O projeto está dividido em duas componentes principais:

- **Apps móveis**: duas aplicações desenvolvidas em .NET MAUI, uma destinada aos clientes e outra aos funcionários. [file:753]
- **Robô físico**: protótipo construído com Arduino Mega 2560, sensores, motores de passo e RFID. [file:753]

Apesar de fazerem parte do mesmo projeto, as apps e o robô foram desenvolvidos como sistemas independentes. As aplicações comunicam entre si através do Firebase Realtime Database, enquanto o robô executa de forma autónoma o ciclo de entrega e regresso. [file:753]

## Funcionalidades principais

### App de clientes

- Consultar o menu organizado por categorias. [file:753]
- Adicionar produtos ao carrinho. [file:753]
- Selecionar o número da mesa. [file:753]
- Adicionar observações ao pedido, como preferências do cliente. [file:753]
- Submeter pedidos para a base de dados em tempo real. [file:753]
- Consultar o histórico e o estado dos próprios pedidos. [file:753]
- Alternar entre tema claro e escuro. [file:753]

### App de funcionários

- Consultar todos os pedidos ativos. [file:753]
- Atualizar o estado dos pedidos ao longo do processo. [file:753]
- Ver detalhes de cada pedido, incluindo itens, total e observações. [file:753]
- Utilizar uma interface simplificada orientada ao acompanhamento operacional. [file:753]
- Alternar entre tema claro e escuro. [file:753]

### Robô de entrega

- Receber o destino através de comando infravermelhos. [file:753]
- Seguir uma linha definida no solo. [file:753]
- Identificar mesas e balcão por RFID. [file:753]
- Detetar obstáculos com sensor ultrassónico. [file:753]
- Confirmar presença ou ausência de carga com microswitch. [file:753]
- Regressar automaticamente ao balcão após a entrega. [file:753]

## Tecnologias utilizadas

### Software

- **.NET MAUI** para desenvolvimento multiplataforma das aplicações móveis. [file:753]
- **C#** como linguagem principal das apps. [file:753]
- **MVVM** como padrão arquitetural. [file:753]
- **CommunityToolkit.Mvvm** para apoiar a implementação do padrão MVVM. [file:753]
- **Firebase Realtime Database** para armazenamento e sincronização de dados em tempo real. [file:753]

### Hardware e firmware

- **Arduino Mega 2560 Rev3** como unidade de controlo principal do robô. [file:753]
- **Arduino IDE** para programação do protótipo. [file:753]
- **SPI.h**, **MFRC522.h**, **IRremote.hpp** e **Stepper.h** como bibliotecas principais do código do robô. [file:753]

## Estrutura do repositório

```text
Carry-On-sistema-inteligente-de-bar/
├── apps/
│   ├── BarPedidos/
│   └── BarPedidosFuncionarios/
├── robo/
├── README.md
└── outros ficheiros de apoio
```

De acordo com a organização descrita no relatório, a pasta `apps/` contém o código-fonte das duas aplicações móveis, enquanto a pasta `robo/` contém o código-fonte do protótipo físico e ficheiros de apoio relacionados com a montagem e organização dos componentes. O ficheiro `README.md` apresenta a descrição do projeto, tecnologias utilizadas e instruções básicas de utilização. [file:753]

## Arquitetura das aplicações

As duas aplicações seguem o padrão **MVVM (Model-View-ViewModel)**, o que permite separar de forma clara os dados, a lógica e a interface. Esta organização torna o código mais fácil de compreender, manter e expandir. [file:753]

A estrutura principal das apps inclui:

- **Models** para representar entidades como produtos, pedidos e itens do carrinho. [file:753]
- **Services** para acesso ao Firebase, gestão do carrinho, tema e limpeza de pedidos. [file:753]
- **ViewModels** para controlo da lógica de cada página. [file:753]
- **Views** para a interface gráfica apresentada ao utilizador. [file:753]
- **Dependency Injection** configurada em `MauiProgram.cs`. [file:753]

## Funcionamento do robô

O robô foi desenvolvido para executar um ciclo simples de entrega. Depois de o funcionário colocar a bebida na bandeja e selecionar a mesa através do comando remoto, o sistema inicia o percurso, segue a linha no solo e monitoriza continuamente obstáculos e etiquetas RFID. [file:753]

Quando a etiqueta RFID da mesa selecionada é identificada, o robô pára no local correto. Após a retirada da bebida, o sistema inicia automaticamente o regresso ao balcão, terminando a viagem ao reconhecer a etiqueta correspondente ao ponto de origem. [file:753]

## Componentes principais do robô

| Componente | Função no sistema |
|---|---|
| Arduino Mega 2560 Rev3 | Executa o programa e coordena sensores e atuadores. [file:753] |
| Leitor RFID RC522 | Identifica o balcão e as mesas através de etiquetas RFID. [file:753] |
| Sensor ultrassónico | Deteta obstáculos à frente do robô. [file:753] |
| Sensor de linha | Permite seguir o percurso marcado no solo. [file:753] |
| Recetor de infravermelhos | Recebe os comandos enviados pelo controlo remoto. [file:753] |
| Microswitch | Verifica a presença ou ausência de carga na bandeja. [file:753] |
| Motores de passo 28BYJ-48 | Asseguram a locomoção do robô. [file:753] |
| Powerbank | Alimenta o sistema eletrónico. [file:753] |

## Como utilizar

### Aplicações móveis

1. Abrir a aplicação de clientes para consultar produtos e criar pedidos. [file:753]
2. Abrir a aplicação de funcionários para acompanhar e atualizar os pedidos. [file:753]
3. Verificar a sincronização dos dados no Firebase Realtime Database. [file:753]

### Robô

1. Ligar o sistema e confirmar a alimentação do Arduino e dos restantes módulos. [file:753]
2. Colocar a bebida na bandeja superior. [file:753]
3. Selecionar a mesa no comando remoto. [file:753]
4. Aguardar a deslocação automática até à mesa. [file:753]
5. Retirar a bebida para permitir o regresso ao balcão. [file:753]

## Resultados alcançados

Durante os testes realizados foi possível validar o carregamento dos produtos, a criação de pedidos, a alteração de estados na app de funcionários, a leitura de tags RFID, a deteção de obstáculos e o regresso automático ao balcão. Estes resultados confirmaram o funcionamento global da solução desenvolvida. [file:753]

O projeto permitiu demonstrar que é possível combinar aplicações móveis, uma base de dados em tempo real e um protótipo robótico num sistema coerente de apoio ao serviço em ambientes de restauração. [file:753]

## Trabalho futuro

Entre as melhorias futuras identificadas encontram-se a integração direta entre as aplicações e o robô, o aumento da autonomia do protótipo, a melhoria da precisão da navegação e a expansão do sistema para mais mesas e funcionalidades de gestão. [file:753]

## Autor

**Salvador Martins** — Projeto PAP, Curso Técnico de Gestão de Equipamentos Informáticos, Escola Técnica e Profissional do Ribatejo. [file:753]

## Repositório

Código-fonte do projeto: [Carry-On-sistema-inteligente-de-bar](https://github.com/salvadoreei/Carry-On-sistema-inteligente-de-bar) [file:753]
