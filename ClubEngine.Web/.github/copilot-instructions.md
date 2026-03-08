# ClubEngine Web - Copilot Instructions

## Architecture Overview
ClubEngine is an **Angular 19 club management application** built on the **Fuse Admin Template** with a multi-tenant architecture using "partitions" (club instances).

### Core Structure
- **Fuse Framework** (`src/@fuse/`): Custom Angular framework providing UI components, services, and theming
- **App Engine** (`src/app/app-engine/`): Domain-specific business logic and infrastructure
- **Admin Modules** (`src/app/modules/admin/`): Feature modules for club management (members, accounting, etc.)
- **API Layer** (`src/app/api/api.ts`): Auto-generated TypeScript client (NSwag) for backend API

### Multi-Tenant Navigation Pattern
All routes include partition (club) acronym: `/{clubAcronym}/members`, `/{clubAcronym}/accounting/settle-payments/{id}`

The `NavigatorService` implements `AppEngineNavigator` abstract class for tenant-aware routing:
```typescript
// Always use NavigatorService for navigation, not Router directly
goToMember(memberId: string): void {
  this.router.navigate(['/', this.partitionService.selected.acronym, 'members', memberId]);
}
```

## Key Patterns & Conventions

### Service Architecture
- **Root-provided services**: All services use `providedIn: 'root'` - no module-specific providers
- **Partition awareness**: Most services depend on `PartitionService.selected.acronym` for API calls
- **Resolver pattern**: Route data loaded via Angular resolvers (e.g., `MemberDetailsService`, `SettlePaymentResolver`)

### Component Lifecycle
- **OnInit/OnDestroy**: Standard lifecycle for subscriptions and cleanup
- **Signal inputs**: New components use Angular signal-based inputs: `input<MemberDto>()`
- **Change detection**: Performance-critical components use `OnPush` strategy

### API Integration
- **Generated client**: Never modify `src/app/api/api.ts` - it's auto-generated from OpenAPI
- **Base URL injection**: API calls use `API_BASE_URL` token configured per environment
- **Auth integration**: Auth0 + SignalR for real-time updates

## Development Workflows

### Build & Serve
```powershell
npm start           # Development server (localhost:4200)
ng build           # Production build
ng build --watch   # Watch mode for development
```

### Component Generation
```powershell
ng generate component modules/admin/feature-name
```
Always generate components in appropriate `modules/admin/` subdirectories.

### Key Files for New Features
1. **Route definition**: Add to `app.routes.ts` with resolver
2. **Service**: Create in same directory as component
3. **Navigation**: Add methods to `NavigatorService`
4. **Translations**: Update `public/i18n/en.json` and German translations

## Authentication & Authorization
- **Auth0 integration**: Configured in `app-engine.provider.ts`
- **Route guards**: Use `AuthGuard` from `@auth0/auth0-angular`
- **API authentication**: Automatic token injection via `authHttpInterceptorFn`

## Static Web App Deployment
- **Configuration**: `staticwebapp.config.json` handles SPA routing
- **Environment**: Production points to `https://api.clubengine.ch`
- **Build output**: `dist/fuse/` directory for deployment

## Fuse Framework Integration
- **Theme system**: Colors and layouts configured via `FuseConfigService`
- **Components**: Use Fuse components (`@fuse/components/`) over raw Angular Material
- **Loading states**: `FuseLoadingService` with HTTP interceptor for automatic loading indicators
- **Confirmation dialogs**: `FuseConfirmationService` for user confirmations

## Critical Dependencies
- **Angular 19** with standalone components
- **Angular Material 19** for UI primitives
- **Auth0 Angular** for authentication
- **SignalR** for real-time notifications
- **Transloco** for internationalization (German/English)
- **Tailwind CSS** for styling

## Common Gotchas
- Never import from barrel exports in `@fuse/` - use specific paths
- Always check `PartitionService.selected` before making tenant-specific API calls
- Use `NavigatorService` instead of `Router` for cross-partition navigation
- Component routes must include partition acronym resolver for proper multi-tenancy