<template>
  <div class="d-flex justify-content-center align-items-center w-100 vh-100 fixed inset-0">
    <div class="d-flex flex-column border p-4 bg-white shadow-lg rounded-lg">
      <div class="flex justify-center">
        <h1 class="text-2xl font-bold text-center text-gray-800 mb-4">
          Login to MotoDataLogger
        </h1>
      </div>
      <form @submit.prevent="handleLogin">
        <div class="mb-2">
          <label for="email" class="block text-gray-700 text-sm font-bold mb-2">Email</label>
          <div class="p-input-icon-left w-full">
            <IconField>
              <InputIcon class="pi pi-envelope" />
              <InputText type="email" id="email" v-model="email" class="w-full" placeholder="your@email.com" required />
            </IconField>
          </div>
          <p v-if="errors.email" class="text-red-500 text-xs mt-1">{{ errors.email }}</p>
        </div>

        <div class="mb-2">
          <label for="password" class="block text-gray-700 text-sm font-bold mb-2">Password</label>
          <div class="p-input-icon-left w-full">
            <IconField>
              <InputIcon class="pi pi-lock" />
              <Password :type="showPassword ? 'text' : 'password'" id="password" v-model="password" class="w-full"
                placeholder="••••••••" :feedback="false" toggleMask required>
              </Password>
            </IconField>
          </div>
          <p v-if="errors.password" class="text-red-500 text-xs mt-1">{{ errors.password }}</p>
        </div>

        <div class="flex items-center justify-between mb-2">
          <div class="flex items-center">
            <Checkbox inputId="remember" v-model="remember" :binary="true"
              class="text-gray-700 border-gray-300 rounded focus:ring-gray-700" />
            <label for="remember" class="ms-1 block text-sm text-gray-700">Remember me</label>
          </div>

          <a href="#" class="text-sm text-gray-700 hover:text-gray-900">Forgot password?</a>
        </div>

        <Button type="submit" class="w-100" :disabled="isLoading" :loading="isLoading">
          <template #loadingicon>
            <i class="pi pi-spinner pi-spin"></i>
          </template>
          <span v-if="isLoading">Logging in...</span>
          <span v-else>Login</span>
        </Button>
      </form>

      <p class="text-center mt-6 text-sm text-gray-600">
        Don't have an account?
        <a href="#" @click.prevent="$emit('switch-form', 'signup')" class="text-gray-800 font-semibold hover:underline">
          Sign up
        </a>
      </p>
    </div>
  </div>
</template>


<script setup>
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import axios from 'axios';

const router = useRouter();

// Form data
const email = ref('');
const password = ref('');
const remember = ref(false);
const showPassword = ref(false);
const isLoading = ref(false);
const errors = reactive({
  email: '',
  password: '',
});

const validateEmail = (email) => {
  return email.match(
    /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
  );
};

// Form validation
const validateForm = () => {
  let isValid = true;
  errors.email = '';
  errors.password = '';

  console.log(isValid);
  console.log(email.value);
  // Email validation
// Email validation
const emailSuccess = validateEmail(email.value);
  if (!emailSuccess) {
    errors.email = emailSuccess;
    isValid = false;
  }

  console.log(isValid);
  console.log("email validated");

  // Password validation
  if (!password.value) {
    errors.password = 'Password is required';
    isValid = false;
  } else if (password.value.length < 6) {
    errors.password = 'Password must be at least 6 characters';
    isValid = false;
  }
  console.log(isValid);
  console.log("password validated");
  console.log(isValid);

  return isValid;
};

// Form submission
const handleLogin = async () => {
  console.log("validating form");
  if (!validateForm()) return;

  console.log("form validated");

  isLoading.value = true;

  try {
    // API call to login endpoint
    const response = await axios.post('http://localhost:5000/api/Auth/login', { // Replace with your API endpoint
      email: email.value,
      password: password.value,
    });

    // Assuming the API returns a JWT token in the 'token' field
    const token = response.data.token;

    // Store the token in localStorage (or sessionStorage)
    localStorage.setItem('jwtToken', token);

    // Set the token in the axios default headers for future requests
    axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;

    // Redirect to the home page or another protected route
    router.push('/');
  } catch (error) {
    console.error('Login error:', error);
    if (error.response) {
      // The request was made and the server responded with a status code
      // that falls out of the range of 2xx
      console.error('Response data:', error.response.data);
      console.error('Response status:', error.response.status);
      console.error('Response headers:', error.response.headers);
      // You can display an error message to the user based on the response status or data
      if (error.response.status === 401) {
        errors.password = 'Invalid email or password';
      } else {
        errors.password = 'An error occurred during login';
      }
    } else if (error.request) {
      // The request was made but no response was received
      console.error('Request:', error.request);
      errors.password = 'No response from server';
    } else {
      // Something happened in setting up the request that triggered an Error
      console.error('Error:', error.message);
      errors.password = 'An error occurred during login';
    }
  } finally {
    isLoading.value = false;
  }
};
</script>