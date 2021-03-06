<template>
    <div>
        <b-loading :is-full-page="true" :active="!loadedMetadata"></b-loading>
        <PageTitle title="Update Account"></PageTitle>
        <div class="box">
        <div class="columns is-gapless">
            <div class="column">
                <section>
                    <b-field label="Account Number">
                        <b-input 
                            type="text"
                            v-model="account.accountNumber"
                            disabled>
                        </b-input>
                    </b-field>
                    <b-field label="Balance">
                        <b-input 
                            is-numeric
                            v-model="account.balance"
                            icon-pack="fas"
                            icon="dollar-sign"
                            disabled>
                        </b-input>
                    </b-field>
                    <b-field label="Nickname">
                        <b-input 
                            type="text"
                            :placeholder="account.displayName"
                            v-model="nickname"
                            @keyup.native.enter="updateAccount"
                            required>
                        </b-input>
                    </b-field>
                    <b-field label="Image">
                        <div class="columns is-vcentered is-mobile">
                            <div class="column is-narrow">
                                <figure class="image is-96x96">
                                    <img class="is-rounded" :src="imageUrl || 'https://bulma.io/images/placeholders/96x96.png'">
                                </figure>
                            </div>
                            <div class="column" @click="newImage">
                                 <b-icon
                                        pack="fas"
                                        icon="sync-alt"
                                        size="is-large"
                                        :custom-class="changePictureIcon">
                                </b-icon>
                            </div>
                        </div>                    
                    </b-field>
                    <div class="buttons">
                        <b-button @click="updateAccount" type="is-info" :disabled="!validInput" :loading="processing">Update account</b-button>
                        <b-button @click="finish" type="is-danger">Cancel</b-button>
                    </div>
                </section>
            </div>
            <div class="column">
            </div>
            <div class="column">
            </div>
        </div>
        </div>
    </div>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';
import Axios, { AxiosResponse } from 'axios';
import { Account } from '@/account';
import { accountStore } from '@/store/store';
import PageTitle from '@/components/PageTitle.vue';

@Component({
    components: {
        PageTitle,
    },
})
export default class UpdateAccount extends Vue {

    private nickname: string = '';
    private imageUrl: string = '';
    private processing: boolean = false;
    private changingPicture: boolean = false;

    private get changePictureIcon() {
        if(this.changingPicture) {
            return "fa-spin";
        }
        return "fa-refresh";
    }

    private get validInput(): boolean {
        return this.nickname.length > 0;
    }

    private get account(): Account {
        const accountId: number = Number(this.$route.params.accountId);
        return this.$store.getters[`${accountStore}/accounts`].find((acc: Account) => acc.id === accountId);
    }

    private get loadedMetadata(): boolean {
        return this.$store.getters[`${accountStore}/loadedMetadata`] &&
        this.$store.getters[`${accountStore}/loadedBalances`];
    }

    private mounted() {
        this.$store.dispatch(`${accountStore}/getAccounts`, { includeMetadata: true, includeBalances: true } )
        .then((response: any) => {
            this.nickname = this.account.displayName;
            this.imageUrl = this.account.imageUrl;
        });
    }

    private async newImage() {

        this.changingPicture = true;

        const headers = {
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
            },
        };

        this.$http.get('/account/picture', headers)
        .then((response: AxiosResponse) => {
            this.imageUrl = response.data;
            this.changingPicture = false;
        });
    }

    private async updateAccount() {
        this.processing = true;

        const accountMetadata = {
            accountId: this.account.id,
            nickname: this.nickname,
            imageUrl: this.imageUrl,
        };

        this.$store.dispatch(`${accountStore}/updateMetadata`, accountMetadata)
        .then((response: any) => {
            this.processing = false;
            this.finish();    
        });
    }

    private async finish() {
        this.$router.replace('/accounts');
    }

}
</script>

<style scoped>

</style>


