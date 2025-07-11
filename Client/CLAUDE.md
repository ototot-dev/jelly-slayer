# Claude Code Assistant - Jelly Slayer Project

이 문서는 Claude Code Assistant가 Jelly Slayer 프로젝트에서 수행하는 작업들에 대한 가이드라인과 절차를 정리한 문서입니다.

## NPC 생성 절차

기존 NPC를 기반으로 새로운 NPC를 생성할 때 다음 절차를 따라야 합니다.

### 1. 스크립트 파일 생성 (*.cs)

**위치:** `Assets/__Script/Pawn/Npc/{NewNpcName}/`

**작업 내용:**
1. 기존 NPC 디렉토리에서 모든 .cs 파일을 복사
2. 파일명을 새 NPC 이름으로 변경
3. 각 파일 내부의 클래스명을 새 NPC 이름으로 변경
4. 컴포넌트 간 의존성 참조도 새 NPC 클래스명으로 업데이트

**생성되는 파일들:**
- `{NewNpcName}Brain.cs`
- `{NewNpcName}Blackboard.cs`
- `{NewNpcName}Movement.cs`
- `{NewNpcName}AnimController.cs`
- `{NewNpcName}ActionController.cs`
- `{NewNpcName}_ActionTasks.cs`
- `Tutorial{NewNpcName}Brain.cs`

**주의사항:**
- 클래스 내부의 모든 의존성 참조를 새 NPC 클래스로 변경
- RequireComponent 어트리뷰트도 업데이트
- ActionTasks 파일의 네임스페이스를 `Game.NodeCanvasExtension.{NewNpcName}`로 변경

### 2. Visual Scripting 파일 생성 (*.asset)

**위치:** `Assets/__Script/Pawn/Npc/{NewNpcName}/`

**작업 내용:**
1. 기존 NPC의 BT(Behavior Tree) asset 파일들을 복사
2. 파일명을 새 NPC 이름으로 변경
3. 내부 JSON 데이터에서 변수 바인딩 경로를 새 NPC 컴포넌트로 변경

**생성되는 파일들:**
- `{NewNpcName}_Action_BT.asset`
- `{NewNpcName}_Combat_BT.asset`
- `{NewNpcName}_Idle_BT.asset`

**변경해야 할 항목들:**
- `"_propertyPath":"Game.{OldNpcName}*"` → `"_propertyPath":"Game.{NewNpcName}*"`
- `"Game.NodeCanvasExtension.{OldNamespace}.*"` → `"Game.NodeCanvasExtension.{NewNpcName}.*"`

### 3. Animator Controller 파일 생성

**위치:** `Assets/__Script/Pawn/Npc/{NewNpcName}/`

**작업 내용:**
1. 기존 NPC의 Animator Controller 파일을 복사
2. 파일명을 `{NewNpcName}_Animator.controller`로 변경

### 4. Prefab 파일 생성

**위치:** `Assets/__Data/Resources/Pawn/Npc/`

**작업 내용:**
1. 기존 NPC prefab 파일을 새 이름으로 복사
2. Unity GUID 참조를 새 NPC 스크립트들의 GUID로 변경
3. 메인 GameObject 이름을 새 NPC 이름으로 변경
4. Embedded NodeCanvas 스크립트의 변수 바인딩 업데이트

**변경해야 할 GUID들:**
- Brain, Blackboard, Movement, AnimController, ActionController 스크립트 GUID
- BT Asset 파일들의 GUID
- Animator Controller GUID

**변경하지 말아야 할 항목들:**
- Bone 계층구조 (예: `Rapax_` 접두사가 붙은 본 이름들)
- Capsule GameObject 이하의 3D 모델 데이터
- PawnData의 pawnName, displayName (별도 수정 필요)

**Embedded NodeCanvas 스크립트 업데이트:**
- FSMOwner 컴포넌트 내부의 `_boundGraphSerialization`에서 변수 바인딩 경로 변경
- `"_propertyPath":"Game.{OldNpcName}*"` → `"_propertyPath":"Game.{NewNpcName}*"`

### 5. Meta 파일 처리

**주의사항:**
- .meta 파일은 Unity에서 자동 생성되므로 수동으로 복사하지 않음
- Unity가 파일을 인식한 후 자동으로 생성됨

### 예시: Rapax → Therionide

기존 Rapax NPC를 기반으로 Therionide NPC를 생성할 때:

1. **스크립트 변경:**
   ```csharp
   // 변경 전
   public class RapaxBrain : NpcHumanoidBrain
   {
       public RapaxBlackboard BB { get; private set; }
       public RapaxMovement Movement { get; private set; }
   }
   
   // 변경 후  
   public class TherionideBrain : NpcHumanoidBrain
   {
       public TherionideBlackboard BB { get; private set; }
       public TherionideMovement Movement { get; private set; }
   }
   ```

2. **BT Asset 변경:**
   ```json
   // 변경 전
   "_propertyPath":"Game.RapaxBlackboard.IsSpawnFinished"
   
   // 변경 후
   "_propertyPath":"Game.TherionideBlackboard.IsSpawnFinished"
   ```

3. **ActionTasks 네임스페이스 변경:**
   ```csharp
   // 변경 전
   namespace Game.NodeCanvasExtension.RoboSoldier
   
   // 변경 후
   namespace Game.NodeCanvasExtension.Therionide
   ```

이 절차를 따르면 기존 NPC의 모든 기능과 설정을 유지하면서 새로운 이름의 NPC를 생성할 수 있습니다.