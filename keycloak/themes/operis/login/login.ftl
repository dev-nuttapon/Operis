<#import "template.ftl" as layout>
<#import "field.ftl" as field>
<#import "buttons.ftl" as buttons>
<#import "social-providers.ftl" as identityProviders>
<#assign currentLang = 'th'>
<#if locale?? && locale.currentLanguageTag?? && locale.currentLanguageTag?lower_case?starts_with('en')>
  <#assign currentLang = 'en'>
</#if>
<@layout.registrationLayout displayMessage=!messagesPerField.existsError('username','password') displayInfo=true; section>
<!-- template: login.ftl (operis custom) -->

    <#if section = "header">
        ${msg("loginAccountTitle")}
    <#elseif section = "form">
        <div class="oi-stars" aria-hidden="true"></div>
        <div class="oi-wave-bg" aria-hidden="true">
          <svg class="oi-waves" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 24 150 28" preserveAspectRatio="none" shape-rendering="auto">
            <defs>
              <path id="oi-gentle-wave" d="M-160 44c30 0 58-18 88-18s58 18 88 18 58-18 88-18 58 18 88 18v44h-352z"></path>
            </defs>
            <g class="oi-parallax">
              <use xlink:href="#oi-gentle-wave" x="48" y="0"></use>
              <use xlink:href="#oi-gentle-wave" x="48" y="3"></use>
              <use xlink:href="#oi-gentle-wave" x="48" y="5"></use>
              <use xlink:href="#oi-gentle-wave" x="48" y="7"></use>
            </g>
          </svg>
        </div>
        <div class="oi-top-controls">
          <div class="oi-theme-switch" role="group" aria-label="${msg('themeSwitch')}">
            <button type="button" class="oi-switch-btn" data-oi-theme-option="system">💻 ${msg("themeSystem")}</button>
            <button type="button" class="oi-switch-btn" data-oi-theme-option="light">☀️ ${msg("themeLight")}</button>
            <button type="button" class="oi-switch-btn" data-oi-theme-option="dark">🌙 ${msg("themeDark")}</button>
          </div>
          <div class="oi-lang-switch" role="group" aria-label="${msg('languageSwitch')}">
            <button type="button" class="oi-switch-btn <#if currentLang = 'th'>is-active</#if>" data-oi-locale="th">TH</button>
            <button type="button" class="oi-switch-btn <#if currentLang = 'en'>is-active</#if>" data-oi-locale="en">EN</button>
          </div>
        </div>
        <div id="kc-form">
          <div id="kc-form-wrapper">
            <#if realm.password>
                <form id="kc-form-login" class="${properties.kcFormClass!}" onsubmit="login.disabled = true; return true;" action="${url.loginAction}" method="post" novalidate="novalidate">
                    <#if !usernameHidden??>
                        <#assign label>
                            <#if !realm.loginWithEmailAllowed>${msg("username")}<#elseif !realm.registrationEmailAsUsername>${msg("usernameOrEmail")}<#else>${msg("email")}</#if>
                        </#assign>
                        <@field.input name="username" label=label autofocus=true autocomplete="username" value=login.username!'' />
                    </#if>

                    <@field.password name="password" label=msg("password") forgotPassword=false autofocus=usernameHidden?? autocomplete="current-password" />

                    <div class="${properties.kcFormGroupClass!}">
                        <#if realm.rememberMe && !usernameHidden??>
                            <@field.checkbox name="rememberMe" label=msg("rememberMe") value=login.rememberMe?? />
                        </#if>
                    </div>

                    <input type="hidden" id="id-hidden-input" name="credentialId" <#if auth.selectedCredential?has_content>value="${auth.selectedCredential}"</#if>/>
                    <@buttons.loginButton />
                </form>

                <#assign appBase = properties.oiAppBaseUrl!'http://localhost:5173'>
                <#assign registerUrl = appBase + '/register'>
                <#assign forgotUrl = url.loginResetCredentialsUrl>
                <div id="oi-custom-links" class="oi-custom-links">
                  <a class="oi-custom-link" href="${registerUrl}" target="_self">${msg("registerOnApp")}</a>
                  <a class="oi-custom-link" href="${forgotUrl}" target="_self">${msg("forgotPasswordOnApp")}</a>
                </div>
            </#if>
            </div>
        </div>
    <#elseif section = "socialProviders" >
        <#if realm.password && social.providers?? && social.providers?has_content>
            <@identityProviders.show social=social/>
        </#if>
    </#if>

</@layout.registrationLayout>
