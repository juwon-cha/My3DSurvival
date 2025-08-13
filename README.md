# My 3D Survival

## 프로젝트 개요

이 프로젝트는 Unity 엔진을 사용하여 개발된 3인칭 서바이벌 게임이다. 플레이어는 동적인 환경을 탐험하고 아이템을 사용하며, 다양한 상호작용이 가능한 오브젝트와 함정을 마주하게 된다. Unity의 `Input System`, `Rigidbody`, `Raycast`, `ScriptableObject` 등 핵심 기능을 학습하고 응용하는 것을 목표로 한다.

개발 인원: 1인 개발
<br>
개발 기간: 2025.08.06 - 2025.08.13
<br>
Unity Editor Version: 2022.3.17f1

## 주요 기능

### 주요 기능 상세

-   **기본 이동 및 점프** (`PlayerController.cs`)
    -   **이동**: `FixedUpdate`에서 `Rigidbody.velocity`를 직접 제어하여 지상 이동을 구현하고 공중에서는 `Rigidbody.AddForce`를 통해 제어력을 제공한다.
    -   **점프**: `OnJump` 콜백에서 `_isGrounded` 상태를 확인 후 `Rigidbody.AddForce(Vector2.up * JumpForce, ForceMode.Impulse)`를 통해 점프를 실행한다.

-   **체력/허기/스태미나 UI** (`PlayerCondition.cs`, `Condition.cs`, `UICondition.cs`)
    -   `PlayerCondition`에서 체력, 허기, 스태미나의 수치를 관리하며 각 상태는 `Condition` 클래스로 정의된다. `Condition` 클래스의 `Update`에서 `UIBar.fillAmount`를 지속적으로 갱신하여 UI에 반영한다.
    -   `PlayerCondition`의 `OnTakeDamage` 이벤트를 `DamageIndicator`가 구독하여 피격 시 화면에 붉은 섬광 효과(`FadeAway` 코루틴)를 표시한다.

-   **동적 환경 조사 및 상호작용** (`Interaction.cs`, `IInteractable`)
    -   `Interaction.cs`의 `Update`에서 `Camera.ScreenPointToRay`를 사용해 화면 중앙에 `Raycast`를 발사한다.
    -   `IInteractable` 인터페이스를 구현한 오브젝트(ex: `ItemObject`, `WallObject`)가 감지되면 해당 오브젝트의 `GetInteractPrompt()`를 호출하여 UI에 상호작용 텍스트를 표시한다.
    -   입력 시 `OnInteract()`를 호출하여 각 오브젝트의 고유한 상호작용(아이템 줍기 등)을 실행한다.

-   **점프대** (`JumpCube.cs`)
    -   `OnCollisionEnter`에서 충돌한 오브젝트가 "Player" 태그를 가졌는지 확인하고 `Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Impulse)`를 통해 플레이어를 즉시 위로 띄워 올린다.

-   **아이템 시스템** (`ItemData.cs`, `UIInventory.cs`)
    -   **데이터 관리**: `ScriptableObject`를 상속받는 `ItemData`를 통해 아이템의 종류, 속성, 장착/소비 효과, 프리팹 등을 정의한다.
    -   **아이템 사용**: `UIInventory.cs`에서 아이템 사용 시 `ItemData`에 정의된 `Consumables` 배열을 순회하며 `PlayerCondition`의 해당 능력치(체력 회복, 속도 증가 등)를 `Coroutine`으로 실행한다.

-   **3인칭 시점 및 캐릭터 회전** (`PlayerController.cs`)
    -   **카메라**: `LateUpdate`에서 마우스 입력을 받아 `_cameraTargetYaw` (좌우), `_cameraTargetPitch` (상하) 값을 갱신하고 이를 `CameraContainer`의 `rotation`에 적용하여 3인칭 시점을 구현한다.
    -   **캐릭터 회전**: `HandleRotation` 함수에서 카메라의 현재 Y축 각도(`_cameraTargetYaw`)와 플레이어의 입력 방향을 조합하여 목표 회전 각도를 계산하고 `Mathf.SmoothDampAngle`을 이용해 캐릭터를 부드럽게 해당 방향으로 회전시킨다.

-   **움직이는 플랫폼** (`MovingPlatform.cs`, `MovingPlatformHandler.cs`)
    -   `MovingPlatform.cs`의 `FixedUpdate`에서 `Mathf.PingPong`과 `Vector3.Lerp`를 사용하여 시작점과 끝점 사이를 부드럽게 왕복 운동한다.
    -   플레이어에 부착된 `MovingPlatformHandler.cs`가 `OnCollisionEnter` 시 `transform.SetParent(collision.transform)`를 호출하여 플레이어를 플랫폼의 자식으로 만들어 함께 움직이게 한다.

-   **벽 타기 및 매달리기** (`PlayerController.cs`)
    -   `HandleClimbing`에서 `Raycast`를 통해 벽을 감지(`CheckWall`)하고, 벽에 붙어 이동할 수 있다. 이때 `Vector3.Cross`를 사용해 벽면에 평행한 이동 벡터를 계산한다.
    -   가슴과 머리 위치에서 두 개의 `Raycast`를 사용(`CheckWallEdge`)하여 벽의 가장자리를 감지하면, `MantleCoroutine`을 실행하여 `Vector3.Lerp`로 캐릭터를 벽 위로 부드럽게 올려준다.

-   **장비 장착 시스템** (`Equipment.cs`, `EquipTool.cs`)
    -   `Equipment.cs`의 `EquipNew` 함수가 호출되면, `ItemData`에 지정된 `EquipPrefab`을 `WeaponSocket` 또는 `HelmetSocket`에 생성하여 장착한다.
    -   장비의 버프 효과는 `PlayerController`의 이동 속도 같은 스탯을 직접 수정하는 방식으로 적용하고, `UnEquip` 시 이전 값으로 복원한다.

-   **레이저 트랩** (`LaserTrap.cs`)
    -   `CheckPlayerRoutine` 코루틴이 주기적으로 원형(`_numberOfRays` 개수만큼)으로 `Raycast`를 발사하여 플레이어를 감지한다.
    -   플레이어가 감지되면 `WarningIndicator`의 `StartWarning`을 호출하여 UI에 경고(깜빡이는 효과)를 표시하고, 감지가 해제되면 `StopWarning`을 호출한다.

-   **플랫폼 발사기** (`PlatformLauncher.cs`)
    -   플레이어가 발판 위에 머무르면(`OnCollisionEnter`), `LaunchPlayer` 코루틴이 `_launchDelay` 시간만큼 대기한 후 `PlayerController`의 `Launch` 함수를 호출한다.
    -   `PlayerController.Launch`는 `_isMovementLocked` 플래그를 활성화하여 플레이어의 조작을 일시적으로 막고, `Rigidbody.AddForce`를 `ForceMode.Impulse`로 사용하여 지정된 방향으로 강력하게 발사한다.

-   **AI (NPC)** (`NPC.cs`)
    -   `EAIState` (Idle, Wandering, Attacking)를 사용하는 간단한 상태 머신으로 작동한다.
    -   `NavMeshAgent`를 사용하여 경로를 탐색하며, `Wandering` 상태에서는 `NavMesh.SamplePosition`으로 임의의 목적지를 설정하여 순찰한다.
    -   플레이어가 `DetectDistance` 내로 들어오면 `Attacking` 상태로 전환하여 추격하고, `AttackDistance` 내에 있고 시야각(`FieldOfView`) 안에 있으면 공격을 수행한다.

## 디렉토리 구조

-   **`Assets/`**: 게임의 모든 리소스가 포함된 메인 폴더이다.
    -   **`Animations/`**: 캐릭터 및 오브젝트의 애니메이션 컨트롤러와 클립이 저장되어 있다.
    -   **`Externals/`**: 외부에서 가져온 에셋 (모델, 텍스처, 사운드 등)을 관리한다.
    -   **`InputActions/`**: `Input System`을 위한 입력 액션 파일을 관리한다.
    -   **`Materials/`**: 게임에 사용되는 머티리얼 파일이 저장되어 있다.
    -   **`Prefabs/`**: 재사용 가능한 게임 오브젝트(플레이어, 아이템, UI 등)를 관리한다.
    -   **`Scenes/`**: 메인 게임 씬(`My3DSurvival.unity`)이 포함되어 있다.
    -   **`Scripts/`**: 모든 C# 스크립트 파일이 기능별로 분류되어 저장되어 있다.
        -   `Player/`: 플레이어 관련 스크립트
        -   `Item/`: 아이템 데이터 및 로직 관련 스크립트
        -   `UI/`: UI 관리 스크립트
        -   `NPC/`: AI 캐릭터 관련 스크립트
-   **`ProjectSettings/`**: Unity 프로젝트의 전반적인 설정 파일들을 포함한다.
-   **`Packages/`**: `manifest.json`을 통해 프로젝트에 사용된 Unity 패키지들을 관리한다.

## 🛠️ 실행 방법

1.  이 저장소를 클론(Clone)한다.
2.  Unity Hub를 열고 'Add project from disk'를 선택하여 이 프로젝트 폴더를 추가한다.
3.  프로젝트를 Unity Editor에서 연다. (권장 Unity 버전 확인 필요)
4.  `Assets/Scenes/My3DSurvival.unity` 씬을 연다.
5.  Unity Editor 상단의 ▶ (플레이) 버튼을 눌러 게임을 실행한다.