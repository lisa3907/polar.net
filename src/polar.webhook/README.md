# Polar Payment Sandbox Test

Polar.sh 결제 서비스를 테스트하기 위한 샌드박스 환경 애플리케이션입니다.

## 시작하기

### 1. 사전 요구사항

- .NET 9.0 SDK
- Polar 샌드박스 계정 (https://sandbox.polar.sh)

### 2. Polar 설정

1. [Polar Sandbox](https://sandbox.polar.sh)에 가입하세요
2. Organization을 생성하거나 선택하세요
3. Access Token 생성:
   - Organization 페이지로 이동 (https://sandbox.polar.sh/[your-org-name])
   - Settings 탭 클릭
   - "Access Tokens" 섹션에서 "Create new token" 클릭
   - Token 이름 입력 (예: "Test API")
   - 필요한 권한 선택 (products:read, checkouts:write 등)
   - 생성된 토큰을 안전하게 복사 (한 번만 표시됨!)
4. Organization ID 확인:
   - Organization 설정 페이지에서 ID 확인
   - 또는 URL에서 확인: https://sandbox.polar.sh/[org-name] 
5. 테스트용 제품과 가격을 생성하세요:
   - Products 메뉴에서 제품 생성
   - 일회성 결제(One-time) 또는 구독(Recurring) 가격 설정

### 3. 애플리케이션 설정

`appsettings.json` 파일을 수정하세요:

```json
{
  "Polar": {
    "AccessToken": "polar_oat_YOUR_SANDBOX_TOKEN_HERE",
    "WebhookSecret": "whsec_YOUR_WEBHOOK_SECRET_HERE",
    "UseSandbox": true,
    "OrganizationId": "YOUR_ORGANIZATION_ID_HERE"
  }
}
```

### 4. 실행

```bash
# 프로젝트 디렉토리로 이동
cd tests/polar

# 패키지 복원
dotnet restore

# 애플리케이션 실행
dotnet run
```

애플리케이션이 시작되면:
- 메인 페이지: https://localhost:7123/polar
- API 문서: https://localhost:7123/swagger
- 관리자 대시보드: https://localhost:7123/polar/admin

## 기능

### 1. 결제 테스트
- **제품 목록 표시**: Polar에서 생성한 제품과 가격 표시
- **체크아웃 생성**: 선택한 제품에 대한 결제 세션 생성
- **결제 처리**: Stripe를 통한 안전한 결제 처리
- **결제 확인**: 결제 완료 후 상태 확인

### 2. 관리자 대시보드
- **고객 목록**: 결제한 고객 정보 조회
- **구독 관리**: 활성 구독 상태 확인
- **통계 확인**: 전체 고객 수, 구독 수 등

### 3. 웹훅 처리
- **실시간 이벤트**: 결제 및 구독 이벤트 실시간 수신
- **서명 검증**: StandardWebhooks를 사용한 안전한 검증
- **이벤트 처리**: checkout, order, subscription, customer 이벤트 처리

### 4. API 엔드포인트
- `GET /api/polar/products` - 제품 목록 조회
- `POST /api/polar/checkout` - 체크아웃 세션 생성
- `GET /api/polar/checkout/{id}` - 체크아웃 상태 확인
- `GET /api/polar/customers` - 고객 목록 조회
- `GET /api/polar/subscriptions` - 구독 목록 조회
- `POST /api/webhook/polar` - 웹훅 수신


## 테스트 시나리오

### 1. 기본 결제 테스트
1. 애플리케이션 실행 후 https://localhost:7123/polar 접속
2. 제품 목록에서 "Purchase" 버튼 클릭
3. Polar 체크아웃 페이지로 리디렉션
4. 테스트 카드 정보 입력:
   - 카드 번호: `4242 4242 4242 4242`
   - 만료일: 미래 날짜 (예: 12/25)
   - CVC: 임의 3자리 (예: 123)
5. 결제 완료 후 성공 페이지로 리디렉션

### 2. 수동 체크아웃 테스트
1. Manual Checkout Test 폼 사용
2. Product Price ID 입력 (Polar 대시보드에서 확인)
3. 선택적으로 고객 정보 입력
4. "Create Test Checkout" 클릭

### 3. 웹훅 테스트 (ngrok 사용)
```bash
# ngrok 설치 (없는 경우)
# https://ngrok.com/download

# ngrok으로 로컬 서버 노출
ngrok http 7123

# Polar 대시보드에서 웹훅 URL 설정
# https://your-ngrok-url.ngrok.io/api/webhook/polar
```

## 주요 코드 구조

```
tests/polar/
├── Controllers/
│   ├── PolarController.cs      # API 엔드포인트
│   ├── WebhookController.cs    # 웹훅 처리
│   └── PagesController.cs      # 웹 페이지 렌더링
├── Models/
│   └── PolarModels.cs          # 데이터 모델
├── Services/
│   └── PolarService.cs         # Polar API 통합 서비스
├── Views/Pages/
│   ├── Index.cshtml            # 메인 페이지
│   ├── Success.cshtml          # 결제 성공 페이지
│   ├── Cancel.cshtml           # 결제 취소 페이지
│   └── Admin.cshtml            # 관리자 대시보드
├── Properties/
│   └── launchSettings.json    # 실행 설정
├── appsettings.json            # 애플리케이션 설정
├── polar.csproj                # 프로젝트 파일
└── Program.cs                  # 진입점
```

## 문제 해결

### 1. "401 Unauthorized" 오류
- Access Token이 올바른지 확인
- 샌드박스/프로덕션 환경 설정 확인

### 2. 제품이 표시되지 않음
- Polar 대시보드에서 제품 생성 확인
- Organization ID가 올바른지 확인
- 제품이 아카이브되지 않았는지 확인

### 3. 웹훅이 수신되지 않음
- ngrok 또는 유사한 터널링 도구 사용
- Polar 대시보드에서 웹훅 URL 설정 확인
- Webhook Secret 설정 확인

## 참고 자료

- [Polar API 문서](https://docs.polar.sh/api)
- [Polar 샌드박스](https://sandbox.polar.sh)
- [StandardWebhooks](https://github.com/standard-webhooks/standard-webhooks)

## 라이선스

이 프로젝트는 테스트 및 학습 목적으로 제공됩니다.

